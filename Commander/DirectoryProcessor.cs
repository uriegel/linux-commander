using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

record DirectoryItems(IEnumerable<DirItem> Dirs, IEnumerable<FileItem> Files);
record DirItem(string Name, bool IsHidden, bool IsDirectory);
record FileItem(string Name, bool IsHidden, long Time, long Size);
static class DirectoryProcessor
{
    public static DirectoryItems GetItems(string path)
    {
        var di = new DirectoryInfo(path);
        var dirItems = GetSafeItems(() => di.GetDirectories())
            .Select(n => new DirItem(n.Name, (n.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden, true))
            .OrderBy(n => n.Name);
        var fileItems = GetSafeItems(() => di.GetFiles())
            .Select(n => new FileItem(
                n.Name, 
                (n.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden, (n.LastWriteTimeUtc.ToFileTime() / 10000000 - 11644473600) * 1000, 
                n.Length
            ))
            .OrderBy(n => n.Name);
        return new DirectoryItems(dirItems, fileItems);
    }

    static IEnumerable<T> GetSafeItems<T>(Func<IEnumerable<T>> func)
    {
        try 
        {
            return func();
        }
        catch(Exception e)
        {
            Console.Error.WriteLine($"Could not get items: {e}");
            return Enumerable.Empty<T>();
        }
    }
}