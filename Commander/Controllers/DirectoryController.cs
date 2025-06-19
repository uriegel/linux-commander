using CsTools.Extensions;

namespace Commander.Controllers;

class DirectoryController : Controller
{
    public override string Id { get; } = "DIRECTORY";

    public override Task<ChangePathResult> ChangePathAsync(string path)
    {
        cancellation.Cancel();
        cancellation = new();
        var result = GetFiles(path);
        return (result as ChangePathResult).ToAsync();
    }

    public GetFilesResult GetFiles(string path)
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
            => new(Id, info.FullName, dirFileInfo.Directories.Length, dirFileInfo.Files.Length, [
                DirectoryItem.CreateParentItem(),
                .. dirFileInfo.Directories,
                .. dirFileInfo.Files]);
    }

    CancellationTokenSource cancellation = new();
}

record DirectoryItem(
    string Name,
    long Size,
    bool IsDirectory,
    bool IsHidden,
    DateTime? Time
)
{
    public static DirectoryItem CreateParentItem()
        => new(
            "..",
            -1,
            true,
            false,
            null);

    public static DirectoryItem CreateDirItem(DirectoryInfo info)
        => new(
            info.Name,
            -1,
            true,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateFileItem(FileInfo info)
        => new(
            info.Name,
            info.Length,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);
};

record DirFileInfo(
    DirectoryItem[] Directories,
    DirectoryItem[] Files
);

record GetFilesResult(
    string? Controller,
    string Path,
    int DirCount,
    int FileCount,
    DirectoryItem[] Items
)
    : ChangePathResult(Controller, DirCount, FileCount);
