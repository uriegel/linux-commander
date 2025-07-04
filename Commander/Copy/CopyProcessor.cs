using Commander.Controllers;
using Commander.UI;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;

namespace Commander.Copy;

enum SelectedItemsType
{
    None,
    Folder,
    Folders,
    File,
    Files,
    Both
}

class CopyProcessor(string sourcePath, string targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems, bool move)
{
    protected string SourcePath { get => sourcePath; }
    protected string TargetPath { get => targetPath; }
    
    public static CopyProcessor? Current { get; private set; }

    public PrepareCopyResult PrepareCopy()
    {
        if (ProgressContext.Instance.IsRunning || Current != null)
        {
            MainContext.Instance.ErrorText = "Es ist bereits eine Aktion am Laufen";
            return new(SelectedItemsType.None, 0, []);
        }
        Current = this;
        copyItems = MakeCopyItems(MakeSourceCopyItems(selectedItems, sourcePath), targetPath);
        var conflicts = copyItems.Where(n => n.Target != null).ToArray();
        copySize = copyItems.Sum(n => n.Source.Size);
        return new(selectedItemsType, copySize, conflicts);
    }

    public async Task<CopyResult> Copy(CopyRequest data)
    {
        try
        {
            if (data.Cancelled)
                return new CopyResult(true);

            ProgressContext.Instance.SetRunning();
            var index = 0;
            copyItems = data.NotOverwrite ? [.. copyItems.Where(n => n.Target == null)] : copyItems;
            copySize = data.NotOverwrite ? copyItems.Sum(n => n.Source.Size) : copySize;
            var cancellation = ProgressContext.Instance.Start(data.Id, move ? "Verschieben" : "Kopieren", copySize, copyItems.Length);
            var buffer = new byte[15000];
            foreach (var item in copyItems)
            {
                if (cancellation.IsCancellationRequested)
                    throw new TaskCanceledException();
                ProgressContext.Instance.SetNewFileProgress(item.Source.Name, item.Source.Size, ++index);
                if (move)
                    await MoveItem(item, cancellation);
                else
                    await CopyItem(item, buffer, cancellation);
            }

            if (move)
            {
                var dirs = move ? selectedItems.Where(n => n.IsDirectory).Select(n => n.Name) : [];
                dirs.DeleteEmptyDirectories(sourcePath);
            }
            return new CopyResult(false);
        }
        catch (UnauthorizedAccessException)
        {
            MainContext.Instance.ErrorText = "Zugriff verweigert";
            return new CopyResult(false);
        }
        catch
        {
            return new CopyResult(false);
        }
        finally
        {
            ProgressContext.Instance.Stop();
            ProgressContext.Instance.SetRunning(false);
            Current = null;
        }
    }

    protected virtual CopyItem[] MakeCopyItems(IEnumerable<DirectoryItem> items, string targetPath)
        => [.. items.Select(n => CreateCopyItem(n, targetPath))];

    protected virtual CopyItem CreateCopyItem(DirectoryItem source, string targetPath)
    {
        var info = new FileInfo(targetPath.AppendPath(source.Name));
        var target = info.Exists ? DirectoryItem.CreateFileItem(info) : null;
        return new(source, target);
    }

    protected virtual IEnumerable<DirectoryItem> MakeSourceCopyItems(IEnumerable<DirectoryItem> items, string sourcePath)
    {
        var dirs = items
                        .Where(n => n.IsDirectory)
                        .SelectMany(n => Flatten(n.Name, sourcePath));
        var files = items
                        .Where(n => !n.IsDirectory)
                        .SelectFilterNull(n => ValidateFile(n, n.Name, sourcePath));
        return dirs.Concat(files);
    }

    protected virtual async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        var newFileName = targetPath.AppendPath(item.Source.Name);
        var tmpNewFileName = targetPath.AppendPath(item.Source.Name + TMP_POSTFIX);
        {
            using var source = File.OpenRead(sourcePath.AppendPath(item.Source.Name)).WithProgress(ProgressContext.Instance.SetProgress);
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

                var read = await source.ReadAsync(buffer, cancellation);
                if (read == 0)
                    break;
                await target.WriteAsync(buffer.AsMemory(0, Math.Min(read, buffer.Length)), cancellation);
            }
        }
        await Gtk.Dispatch(() =>
            {
                using var gsf = GFile.New(sourcePath.AppendPath(item.Source.Name));
                using var gtf = GFile.New(tmpNewFileName);
                var res = gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
                var ress = res;
            });
        File.Move(tmpNewFileName, newFileName, true);
    }

    Task MoveItem(CopyItem item, CancellationToken cancellation)
        => Gtk.Dispatch(async () =>
        {
            using var file = GFile.New(sourcePath.AppendPath(item.Source.Name));
            await file.MoveAsync(targetPath.AppendPath(item.Source.Name).EnsureFileDirectoryExists(),
                                FileCopyFlags.Overwrite, true, (c, t) => ProgressContext.Instance.SetProgress(t, c), cancellation);
        });

    static IEnumerable<DirectoryItem> Flatten(string item, string sourcePath)
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
            .Select(n => DirectoryItem.CreateCopyFileItem(item.AppendPath(n.Name), n));
        return dirs.Concat(files);
    }

    protected virtual DirectoryItem? ValidateFile(DirectoryItem item, string subPath, string path)
    {
        var info = new FileInfo(path.AppendPath(subPath));
        if (!info.Exists)
            return null;
        return DirectoryItem.CreateFileItem(info);
    }

    protected const string TMP_POSTFIX = "-tmp-commander";

    CopyItem[] copyItems = [];
    long copySize;
}

record CopyItem(DirectoryItem Source, DirectoryItem? Target);

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