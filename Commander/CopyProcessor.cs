using Commander.Controllers;
using CsTools.Extensions;

namespace Commander;

enum SelectedItemsType
{
    None,
    Folder,
    Folders,
    File,
    Files,
    Both
}

class CopyProcessor(string sourcePath, string targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
{
    public static CopyProcessor? Current { get; private set; }

    public PrepareCopyResult PrepareCopy(bool move)
    {
        if (Current != null)
            // TODO Anzeige: Es ist bereits ein Kopiervorgang am Laufen!
            return new(SelectedItemsType.None, 0);
        Current = this;
        var dirs = move ? selectedItems.Where(n => n.IsDirectory).Select(n => n.Name) : [];
        var copyItems = MakeCopyItems(MakeSourceCopyItems(selectedItems, sourcePath), targetPath);
        var conflicts = copyItems.Where(n => n.Target != null).ToArray();
        var size = copyItems.Aggregate(0L, (s, n) => s + n.Source.Size);
        return new(selectedItemsType, size);
    }

    public async Task<CopyResult> Copy(CopyRequest data)
    {
        try
        {
            if (data.Cancelled)
                return null;
            else
                return null;
        }
        finally
        {
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
                        .SelectFilterNull(n => ValidateFile(n.Name, sourcePath));
        return dirs.Concat(files);
    }

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

    static DirectoryItem? ValidateFile(string subPath, string path)
    {
        var info = new FileInfo(path.AppendPath(subPath));
        if (!info.Exists)
            return null;
        return DirectoryItem.CreateFileItem(info);
    }
}

record CopyItem(DirectoryItem Source, DirectoryItem? Target);