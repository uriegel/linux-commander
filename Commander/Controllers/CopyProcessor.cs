using GtkDotNet;

using Commander.Enums;
using Commander.UI;
using Commander.DataContexts;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools;

namespace Commander.Controllers;

class CopyProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
{
    public async Task CopyItems()
    {
        if (targetPath?.StartsWith('/') != true || string.Compare(sourcePath, targetPath, StringComparison.CurrentCultureIgnoreCase) == 0)
            return;
        var text = selectedItemsType switch
        {
            SelectedItemsType.Both => "Möchtest Du die markierten Einträge kopieren?",
            SelectedItemsType.Files => "Möchtest Du die markierten Dateien kopieren?",
            SelectedItemsType.Folders => "Möchtest Du die markierten Verzeichnisse kopieren?",
            SelectedItemsType.File => "Möchtest Du die markierte Datei kopieren?",
            SelectedItemsType.Folder => "Möchtest Du das markierte Verzeichnis kopieren?",
            _ => ""
        };

        var copyItems = selectedItems.Flatten(sourcePath).MakeCopyItems(targetPath);
        var conflicts = copyItems.Where(n => n.Target != null).ToArray();
        if (conflicts.Length > 0)
        {
            var overwrite = await ConflictDialog.PresentAsync(conflicts);
            if (!overwrite)
                copyItems = [.. copyItems.Where(n => n.Target == null)];
        }
        else
        {
            var response = await AlertDialog.PresentAsync("Kopieren?", text);
            if (response != "ok")
                throw new TaskCanceledException();
        }
        try
        {
            var index = 0;
            var cancellation = CopyProgressContext.Instance.Start("Fortschritt beim Kopieren", copyItems.Sum(n => n.Source.Size), copyItems.Length);
            var buffer = new byte[15000];
            foreach (var item in copyItems)
            {
                if (cancellation.IsCancellationRequested)
                    throw new TaskCanceledException();
                CopyProgressContext.Instance.SetNewFileProgress(item.Source.Name, item.Source.Size, ++index);
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
                });
                using var gsf = GFile.New(sourcePath.AppendPath(item.Source.Name));
                using var gtf = GFile.New(tmpNewFileName);
                gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
                File.Move(tmpNewFileName, newFileName, true);

                // TODO Move
                // using var file = GFile.New(CurrentPath.AppendPath(item.Name));
                // await file.CopyAsync(targetPath.AppendPath(item.Name), FileCopyFlags.Overwrite, false, (c, t) => CopyProgressContext.Instance.SetProgress(c));
            }
        }
        finally
        {
            CopyProgressContext.Instance.Stop();
        }
    }

    const string TMP_POSTFIX = "-tmp-commander";
}

record Item(string Name, long Size, DateTime DateTime);
record CopyItem(Item Source, Item? Target);

static partial class Extensions
{
    public static IEnumerable<Item> Flatten(this IEnumerable<DirectoryItem> items, string sourcePath)
    {
        var dirs = items
                        .Where(n => n.IsDirectory)
                        .SelectMany(n => Flatten(n.Name, sourcePath));
        var files = items
                        .Where(n => !n.IsDirectory)
                        .SelectFilterNull(n => ValidateFile(n.Name, sourcePath));
        return dirs.Concat(files);
    }

    public static IEnumerable<Item> Flatten(string item, string sourcePath)
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

    public static CopyItem[] MakeCopyItems(this IEnumerable<Item> items, string targetPath)
        => [.. items.Select(n => n.CreateCopyItem(targetPath))];

    public static CopyItem CreateCopyItem(this Item source, string targetPath)
    {
        var info = new FileInfo(targetPath.AppendPath(source.Name));
        var target = info.Exists ? new Item(info.Name, info.Length, info.LastWriteTime) : null;
        return new(source, target);
    }
}