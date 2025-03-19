namespace Commander;

static class DirectoryProcessing
{
    public static GetFilesResult GetFiles(string path)
    {
        var info = new DirectoryInfo(path);
        return MakeFilesResult(new DirFileInfo(
                    [.. info
                        .GetDirectories()
                        .Select(DirectoryItem.CreateDirItem)
                        .OrderBy(n => n.Name)],
                        info
                        .GetFiles()
                        .Select(DirectoryItem.CreateFileItem)
                        .ToArray()));

        GetFilesResult MakeFilesResult(DirFileInfo dirFileInfo)
            => new([.. dirFileInfo.Directories, .. dirFileInfo.Files],
                    info.FullName,
                    dirFileInfo.Directories.Length,
                    dirFileInfo.Files.Length);
    }
}

record DirectoryItem(
    string Name,
    long Size,
    bool IsDirectory,
    //string? IconPath,
    bool IsHidden,
    DateTime Time
) {
    public static DirectoryItem CreateDirItem(DirectoryInfo info)
        => new(
            info.Name,
            0,
            true,
//            null,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateFileItem(FileInfo info)
        => new(
            info.Name,
            info.Length,
            false,
//            Directory.GetIconPath(info),
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);
    public static DirectoryItem? CreateMaybeFileItem(FileInfo info)
        => info.Exists
            ? new(
                info.Name,
                info.Length,
                false,
  //              Directory.GetIconPath(info),
                (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                info.LastWriteTime)
            : null;
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
