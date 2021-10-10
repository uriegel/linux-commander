using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

record DirectoryItems(IEnumerable<DirItem> Dirs, IEnumerable<FileItem> Files);
record DirItem(string Name, bool IsHidden, bool IsDirectory);
record FileItem(string Name, bool IsHidden, long Time, long Size);
static class DirectoryProcessor
{
    public static DirectoryItems GetItems(string path, bool showHidden, string folderId)
    {
        var di = new DirectoryInfo(path);
        var dirItems = GetSafeItems(() => di.GetDirectories())
            .Select(n => new DirItem(n.Name, (n.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden, true))
            .Where(n => showHidden ? true : !n.IsHidden)
            .OrderBy(n => n.Name);
        var fileItems = GetSafeItems(() => di.GetFiles())
            .Select(n => new FileItem(
                n.Name,
                (n.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden, (n.LastWriteTimeUtc.ToFileTime() / 10000000 - 11644473600) * 1000,
                n.Length
            ))
            .Where(n => showHidden ? true : !n.IsHidden)
            .OrderBy(n => n.Name);
        return new DirectoryItems(dirItems, fileItems);
    }

    public static ExifReturnItem GetExifData(string path, ExifItem exifItem)
    {
        using var exifReader = new ExifReader(Path.Combine(path, exifItem.Name));
        if (!exifReader.GetTagValue<DateTime>(ExifReader.ExifTags.DateTimeOriginal, out var dateTime)
            && !exifReader.GetTagValue<DateTime>(ExifReader.ExifTags.DateTime, out dateTime))
            return null;
        return new ExifReturnItem(exifItem.Index, dateTime);
    }

    public static void CopyFiles(ProcessingQueue processingQueue, FileItems items)
    {
        foreach (var item in items.Items)
            processingQueue.AddJob(
                new ProcessingJob(new [] {items.Id}, ProcessingAction.Copy, Path.Combine(items.SourcePath, item), Path.Combine(items.destinationPath, item))
            );
    }

    public static void MoveFiles(ProcessingQueue processingQueue, FileItems items)
    {
        foreach (var item in items.Items)
            processingQueue.AddJob(
                new ProcessingJob(items.Ids, ProcessingAction.Move, Path.Combine(items.SourcePath, item), Path.Combine(items.destinationPath, item))
            );
    }

    public static void DeleteFiles(ProcessingQueue processingQueue, FileItems items)
    {
        foreach (var item in items.Items)
            processingQueue.AddJob(
                new ProcessingJob(new[] { items.Id }, ProcessingAction.Delete, Path.Combine(items.SourcePath, item), null)
            );
    }

    public static void RenameFile(RenameItem item)
    {
        try
        {
            if (item.isDirectory)
                Directory.Move(Path.Combine(item.Path, item.item), Path.Combine(item.Path, item.newName));
            else
                File.Move(Path.Combine(item.Path, item.item), Path.Combine(item.Path, item.newName));
        }
        catch (IOException ae) when (ae.HResult == 13)
        {
            throw new WebViewException("Zugriff verweigert", new [] { item.Id });
        }
        catch (IOException ive) when (ive.HResult == 22)
        {
            throw new WebViewException("Name ungültig", new [] { item.Id });
        }
        catch (IOException ee) when (ee.HResult == -2146232800)
        {
            throw new WebViewException("Der eingegebene Name unterscheidet sich nicht vom bisherigen", new [] { item.Id });
        }
    }

    public static void CreateFolder(CreateFolder item)
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(item.Path, item.newName));
        }
        catch (UnauthorizedAccessException)
        {
            throw new WebViewException("Zugriff verweigert", new [] { item.Id });
        }
    }

    static IEnumerable<T> GetSafeItems<T>(Func<IEnumerable<T>> func)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Could not get items: {e}");
            return Enumerable.Empty<T>();
        }
    }
}

record GetItems(string Id, int RequestId, string Path, bool HiddenIncluded);
record GetExifs(string Id, int RequestId, string path, ExifItem[] ExifItems);
record ExifItem(int Index, string Name);
record ExifReturnItem(int Index, DateTime ExifTime);
record FileItems(string Id, string[] Ids, string SourcePath, String destinationPath, string[] Items);
record RenameItem(string Id, string Path, string item, string newName, bool isDirectory);
record CreateFolder(string Id, string Path, string newName);