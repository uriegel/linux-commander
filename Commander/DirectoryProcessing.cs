using Commander.Enums;

namespace Commander;

static class DirectoryProcessing
{
    public static GetFilesResult GetFiles(string path)
    {
        var info = new DirectoryInfo(path);
        return MakeFilesResult(new DirFileInfo(
                    [.. info
                        .GetDirectories()
                        .OrderBy(n => n.Name)
                        .Select(DirectoryItem.CreateDirItem)],
                    [.. info
                        .GetFiles()
                        .OrderBy(n => n.Name)
                        .Select(DirectoryItem.CreateFileItem)]));

        GetFilesResult MakeFilesResult(DirFileInfo dirFileInfo)
            => new([
                DirectoryItem.CreateParentItem(),
                .. dirFileInfo.Directories,
                .. dirFileInfo.Files],
                    info.FullName,
                    dirFileInfo.Directories.Length,
                    dirFileInfo.Files.Length);
    }
}

record DirectoryItem(
    ItemKind Kind,
    string Name,
    long Size,
    bool IsDirectory,
    bool IsHidden,
    DateTime? Time
) {
    public ExifData? ExifData { get; set; }

    public static DirectoryItem CreateParentItem()
        => new(
            ItemKind.Parent,
            "..",
            -1,
            true,
            false,
            null);

    public static DirectoryItem CreateDirItem(DirectoryInfo info)
        => new(
            ItemKind.Folder,
            info.Name,
            -1,
            true,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateFileItem(FileInfo info)
        => new(
            ItemKind.Item,
            info.Name,
            info.Length,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);
//     public static DirectoryItem? CreateMaybeFileItem(FileInfo info)
//         => info.Exists
//             ? new(
//                 info.Name,
//                 info.Length,
//                 false,
//   //              Directory.GetIconPath(info),
//                 (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
//                 info.LastWriteTime)
//             : null;
};

record DirFileInfo(
    DirectoryItem[] Directories,
    DirectoryItem[] Files
);

record GetFilesResult(
    DirectoryItem[] Items,
    string Path,
    int DirCount,
    int FileCount
);
