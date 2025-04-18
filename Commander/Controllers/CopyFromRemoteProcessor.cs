using Commander.DataContexts;
using Commander.Enums;
using CsTools;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using GtkDotNet;

using static CsTools.HttpRequest.Core;

namespace Commander.Controllers;

// TODO Rename
// TODO Delete
// TODO createFolder
// TODO Switch phone off (text)

class CopyFromRemoteProcessor : CopyProcessor
{
    public CopyFromRemoteProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
        : base(sourcePath, targetPath, selectedItemsType, selectedItems) { }

    protected override IEnumerable<Item> MakeSourceCopyItems(IEnumerable<DirectoryItem> items, string sourcePath)
        => items.Select(n => new Item(n.Name, n.Size, n.Time ?? DateTime.MinValue));

    protected override async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        var newFileName = targetPath.AppendPath(item.Source.Name);
        var tmpNewFileName = targetPath.AppendPath(item.Source.Name + TMP_POSTFIX);
        long? lastWrite = null;
        await Task.Run(async () =>
        {
            var source = sourcePath.CombineRemotePath(item.Source.Name);

            var msg = await Request
                .Run(sourcePath.GetIpAndPath().GetFile(item.Source.Name), true)
                .HttpGetOrThrowAsync();
            var len = msg.Content.Headers.ContentLength;
            try
            {
                using var target =
                    File
                        .Create(tmpNewFileName.EnsureFileDirectoryExists())
                        .WithProgress((t, c) => CopyProgressContext.Instance.SetProgress(len ?? t, c));
                await msg
                    .CopyToStream(target, cancellation)
                    .HttpGetOrThrowAsync();
                lastWrite = msg.GetHeaderLongValue("x-file-date");
            }
            catch
            {
                try
                {
                    File.Delete(tmpNewFileName);
                }
                catch { }
                throw;
            }
        }, CancellationToken.None);
        if (lastWrite.HasValue)
            File.SetLastWriteTime(tmpNewFileName, lastWrite.Value.FromUnixTime());
        using var gsf = GFile.New(sourcePath.AppendPath(item.Source.Name));
        using var gtf = GFile.New(tmpNewFileName);
        gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
        File.Move(tmpNewFileName, newFileName, true);
    }
}

static partial class Extensions
{
    public static CsTools.HttpRequest.Settings GetFile(this IpAndPath ipAndPath, string name)
        => DefaultSettings with
        {
            Method = HttpMethod.Get,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/downloadfile/{ipAndPath.Path.CombineRemotePath(name)}",
        };
}