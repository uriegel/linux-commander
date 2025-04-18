using GtkDotNet;

using Commander.Enums;
using Commander.UI;
using Commander.DataContexts;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools;

namespace Commander.Controllers;

class CopyProcessor
{
    public CopyProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
    {
        this.sourcePath = sourcePath;
        this.targetPath = targetPath;
        this.selectedItemsType = selectedItemsType;
        this.selectedItems = selectedItems;
    }

    public async Task<bool> CopyItems(bool move)
    {
        if (targetPath?.StartsWith('/') != true
                || string.Compare(sourcePath, targetPath, StringComparison.CurrentCultureIgnoreCase) == 0
                || selectedItemsType == SelectedItemsType.None)
            return false;

        var copyText = move ? "verschieben" : "kopieren";
        var copyTextCapitel = move ? "Verschieben" : "Kopieren";
        var text = selectedItemsType switch
        {
            SelectedItemsType.Both => "Möchtest Du die markierten Einträge " + copyText + "?",
            SelectedItemsType.Files => "Möchtest Du die markierten Dateien " + copyText + "?",
            SelectedItemsType.Folders => "Möchtest Du die markierten Verzeichnisse " + copyText + "?",
            SelectedItemsType.File => "Möchtest Du die markierte Datei " + copyText + "?",
            SelectedItemsType.Folder => "Möchtest Du das markierte Verzeichnis " + copyText + "?",
            _ => ""
        };

        var dirs = move ? selectedItems.Where(n => n.IsDirectory).Select(n => n.Name) : [];
        var copyItems = MakeCopyItems(MakeSourceCopyItems(selectedItems, sourcePath), targetPath);
        var conflicts = copyItems.Where(n => n.Target != null).ToArray();
        if (conflicts.Length > 0)
        {
            var overwrite = await ConflictDialog.PresentAsync(conflicts);
            if (!overwrite)
                copyItems = [.. copyItems.Where(n => n.Target == null)];
        }
        else
        {
            var response = await AlertDialog.PresentAsync(copyTextCapitel + "?", text);
            if (response != "ok")
                throw new TaskCanceledException();
        }
        try
        {
            var index = 0;
            var cancellation = CopyProgressContext.Instance.Start(copyTextCapitel, copyItems.Sum(n => n.Source.Size), copyItems.Length);
            var buffer = new byte[15000];
            foreach (var item in copyItems)
            {
                if (cancellation.IsCancellationRequested)
                    throw new TaskCanceledException();
                CopyProgressContext.Instance.SetNewFileProgress(item.Source.Name, item.Source.Size, ++index);
                if (move)
                    await MoveItem(item, cancellation);
                else
                    await CopyItem(item, buffer, cancellation);
            }

            if (move)
                dirs.DeleteEmptyDirectories(sourcePath);
            return true;
        }
        finally
        {
            CopyProgressContext.Instance.Stop();
        }
    }

    protected virtual IEnumerable<Item> MakeSourceCopyItems(IEnumerable<DirectoryItem> items, string sourcePath)
    {
        var dirs = items
                        .Where(n => n.IsDirectory)
                        .SelectMany(n => Flatten(n.Name, sourcePath));
        var files = items
                        .Where(n => !n.IsDirectory)
                        .SelectFilterNull(n => ValidateFile(n.Name, sourcePath));
        return dirs.Concat(files);
    }

    protected virtual CopyItem[] MakeCopyItems(IEnumerable<Item> items, string targetPath)
        => [.. items.Select(n => CreateCopyItem(n, targetPath))];

    // TODO Check source Items
    protected virtual CopyItem CreateCopyItem(Item source, string targetPath)
    {
        var info = new FileInfo(targetPath.AppendPath(source.Name));
        var target = info.Exists ? new Item(info.Name, info.Length, info.LastWriteTime) : null;
        return new(source, target);
    }

    protected virtual async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        var newFileName = targetPath.AppendPath(item.Source.Name);
        var tmpNewFileName = targetPath.AppendPath(item.Source.Name + TMP_POSTFIX);
        await Task.Run(() =>
        {
            using var source = File.OpenRead(sourcePath.AppendPath(item.Source.Name)).WithProgress(CopyProgressContext.Instance.SetProgress);
            using var target = File.Create(tmpNewFileName.EnsureFileDirectoryExists());
            while (true)
            {
                if (cancellation.IsCancellationRequested)
                {
                    try
                    {
                        File.Delete(tmpNewFileName);
                    }
                    catch { }
                    throw new TaskCanceledException();
                }

                var read = source.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;
                target.Write(buffer, 0, Math.Min(read, buffer.Length));
            }
        }, CancellationToken.None);
        using var gsf = GFile.New(sourcePath.AppendPath(item.Source.Name));
        using var gtf = GFile.New(tmpNewFileName);
        gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
        File.Move(tmpNewFileName, newFileName, true);
    }

    static IEnumerable<Item> Flatten(string item, string sourcePath)
    {
        var info = new DirectoryInfo(sourcePath.AppendPath(item));

        var dirs = info
            .GetDirectories()
            .OrderBy(n => n.Name)
            .Select(n => item.AppendPath(n.Name))
            .SelectMany(n => Flatten(n, sourcePath));

        var files = info
            .GetFiles()
            .OrderBy(n => n.Name)
            .Select(n => new Item(item.AppendPath(n.Name), n.Length, n.LastWriteTime));
        return dirs.Concat(files);

    }

    public static Item? ValidateFile(string subPath, string path)
    {
        var info = new FileInfo(path.AppendPath(subPath));
        if (!info.Exists)
            return null;
        return new Item(info.Name, info.Length, info.LastWriteTime);
    }

    async Task MoveItem(CopyItem item, CancellationToken cancellation)
    {
        using var file = GFile.New(sourcePath.AppendPath(item.Source.Name));
        await file.MoveAsync(targetPath.AppendPath(item.Source.Name).EnsureFileDirectoryExists(),
                                FileCopyFlags.Overwrite, true, (c, t) => CopyProgressContext.Instance.SetProgress(t, c), cancellation);
    }

    public const string TMP_POSTFIX = "-tmp-commander";

    protected string sourcePath;
    protected string? targetPath;
    SelectedItemsType selectedItemsType;
    DirectoryItem[] selectedItems;}

record Item(string Name, long Size, DateTime DateTime);
record CopyItem(Item Source, Item? Target);

static partial class Extensions
{
    public static void DeleteEmptyDirectories(this IEnumerable<string> dirs, string path)
    {
        foreach (var dir in dirs)
        {
            var dirToCheck = path.AppendPath(dir);
            if (dirToCheck.IsDirectoryEmpty())
            {
                try
                {
                    System.IO.Directory.Delete(dirToCheck, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Konnte Ausgangsverzeicnis nicht löschen: {e}");
                }
            }
        }
    }

    static bool IsDirectoryEmpty(this string dir)
    {
        var info = new DirectoryInfo(dir);
        if (info.GetFiles().Length != 0)
            return false;

        var dirs = info
            .GetDirectories()
            .Select(n => n.FullName);
        foreach (var subDir in dirs)
        {
            if (!subDir.IsDirectoryEmpty())
                return false;
        }
        return true;
    }
}