namespace Commander.Controllers;

class DirectoryController(string folderId) : Controller(folderId)
{
    public override string Id { get; } = "DIRECTORY";

    public override async Task<ChangePathResult> ChangePathAsync(string path, bool showHidden)
    {
        var changePathId = Interlocked.Increment(ref ChangePathSeed);
        try
        {
            var cancellation = Cancellations.ChangePathCancellation(FolderId);
            var result = await Task.Run(() => GetFiles(path, showHidden, changePathId, cancellation));
            GetExifData(changePathId, cancellation);
            return result;
        }
        catch (OperationCanceledException)
        {
            return new ChangePathResult(true, changePathId, null, "", 0, 0);
        }
    }

    GetFilesResult GetFiles(string path, bool showHidden, int changePathId, CancellationToken cancellation)
    {
        var info = new DirectoryInfo(path);
        cancellation.ThrowIfCancellationRequested();
        return MakeFilesResult(new DirFileInfo(
                    [.. info
                        .GetDirectories()
                        .Where(n => showHidden || (n.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        .OrderBy(n => n.Name)
                        .Select(DirectoryItem.CreateDirItem)],
                    [.. info
                        .GetFiles()
                        .Where(n => showHidden || (n.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        .OrderBy(n => n.Name)
                        .Select(DirectoryItem.CreateFileItem)]));

        GetFilesResult MakeFilesResult(DirFileInfo dirFileInfo)
            => new(null, changePathId, CheckInitial() ? Id : null, info.FullName, dirFileInfo.Directories.Length, dirFileInfo.Files.Length, [
                DirectoryItem.CreateParentItem(),
                .. dirFileInfo.Directories,
                .. dirFileInfo.Files]);
    }

    async void GetExifData(int changePathId, CancellationToken cancellation)
    {
        try
        {
            await Task.Run(() =>
            {
                Requests.SendStatusBarInfo(FolderId, changePathId, "Ermittle EXIF-Informationen...");
                for (var i = 0; i < 20; i++)
                {
                    Thread.Sleep(250);
                    cancellation.ThrowIfCancellationRequested();
                }
                Requests.SendStatusBarInfo(FolderId, changePathId, null);
            }, cancellation);
        }
        catch
        { 
            Requests.SendStatusBarInfo(FolderId, changePathId, null);
        }
    }

    public static int ChangePathSeed = 0;
}

record DirectoryItem(
    string Name,
    long Size,
    bool IsDirectory,
    bool IsParent,
    bool IsHidden,
    DateTime? Time
)
{
    public static DirectoryItem CreateParentItem()
        => new(
            "..",
            -1,
            true,
            true,
            false,
            null);

    public static DirectoryItem CreateDirItem(DirectoryInfo info)
        => new(
            info.Name,
            -1,
            true,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateFileItem(FileInfo info)
        => new(
            info.Name,
            info.Length,
            false,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);
};

record DirFileInfo(
    DirectoryItem[] Directories,
    DirectoryItem[] Files
);

record GetFilesResult(
    bool? Cancelled,
    int Id,
    string? Controller,
    string Path,
    int DirCount,
    int FileCount,
    DirectoryItem[] Items
)
    : ChangePathResult(Cancelled, Id, Controller, Path, DirCount, FileCount);
