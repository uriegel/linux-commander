using Commander.DataContexts;
using Commander.Enums;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;

namespace Commander.Controllers;

class CopyFromRemoteProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
    : CopyProcessor(sourcePath, targetPath, selectedItemsType, selectedItems)
{
    protected override IEnumerable<Item> MakeSourceCopyItems(IEnumerable<DirectoryItem> items, string sourcePath)
        => items.Select(n => new Item(n.Name, n.Size, n.Time ?? DateTime.MinValue));

    protected override async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        var newFileName = targetPath.AppendPath(item.Source.Name);
        var tmpNewFileName = targetPath.AppendPath(item.Source.Name + TMP_POSTFIX);
        await Task.Run(() =>
        {
            // TODO check
            var source = sourcePath.AppendPath(item.Source.Name);
                    // ).WithProgress(CopyProgressContext.Instance.SetProgress);
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

                //var read = source.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;
                target.Write(buffer, 0, Math.Min(read, buffer.Length));
            }
        }, CancellationToken.None);
        using var gsf = GFile.New(sourcePath.AppendPath(item.Source.Name));
        using var gtf = GFile.New(tmpNewFileName);
        gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
        // TODO File.Move(tmpNewFileName, newFileName, true);
    }
}