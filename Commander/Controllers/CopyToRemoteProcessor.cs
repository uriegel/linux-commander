using Commander.DataContexts;
using Commander.Enums;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;

using static CsTools.HttpRequest.Core;

namespace Commander.Controllers;

// TODO android: GetFilesInfo (with subpath) Conflict by copyToRemote
// TODO android: PostFile (with subpath)

class CopyToRemoteProcessor : CopyProcessor
{
    public CopyToRemoteProcessor(string sourcePath, string? targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems)
        : base(sourcePath, targetPath, selectedItemsType, selectedItems) { }

    protected override CopyItem CreateCopyItem(Item source, string targetPath)
    {
        // TODO O V E R W R I T E possible !!!!!!
        return new(source, null);
        // var info = new FileInfo(targetPath.AppendPath(source.Name));
        // var target = info.Exists ? new Item(info.Name, info.Length, info.LastWriteTime) : null;
        // return new(source, target);
    }

    protected override async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        if (targetPath == null)
            return;
        var ipAndPath = targetPath.GetIpAndPath();
        await Task.Run(async () =>
        {
            using var source =
                File
                    .OpenRead(sourcePath.AppendPath(item.Source.Name))
                    .WithProgress(CopyProgressContext.Instance.SetProgress);
            await Request
                .Run(source.PutFile(ipAndPath, item.Source.Name, item.Source.DateTime), true)
                .HttpGetOrThrowAsync();
        }, CancellationToken.None);
    }
}

static partial class Extensions
{
    public static CsTools.HttpRequest.Settings PutFile(this Stream streamToPost, IpAndPath ipAndPath, string name, DateTime lastWrite)
        => DefaultSettings with
        {
            Method = HttpMethod.Put,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/putfile/{ipAndPath.Path.CombineRemotePath(name)}",
            Timeout = 100_000_000,
            AddContent = () => new StreamContent(streamToPost, 15000)
                                    .SideEffect(n => n.Headers
                                                        .TryAddWithoutValidation(
                                                            "x-file-date",
                                                            new DateTimeOffset(lastWrite).ToUnixTimeMilliseconds().ToString()))
        };
}