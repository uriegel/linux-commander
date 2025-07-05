using Commander.Controllers;
using Commander.UI;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;

using static CsTools.HttpRequest.Core;

namespace Commander.Copy;

class CopyToRemoteProcessor(string sourcePath, string targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems, bool move)
    : CopyProcessor(sourcePath, targetPath, selectedItemsType, selectedItems, move)
{
    // TODO O V E R W R I T E  possible
    protected override CopyItem CreateCopyItem(DirectoryItem source, string targetPath) => new(source, null);

    protected override async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        try
        {
            var ipAndPath = TargetPath.GetIpAndPath();
            using var source =
                File
                    .OpenRead(SourcePath.AppendPath(item.Source.Name))
                    .WithProgress(ProgressContext.Instance.SetProgress);
            await Request
                .Run(source.PutFile(ipAndPath, item.Source.Name, item.Source.Time), true)
                .HttpGetOrThrowAsync();
        }
        catch (RequestException re) when (re.CustomRequestError == CustomRequestError.ConnectionError)
        {
            MainContext.Instance.ErrorText = "Die Verbindung zum Gerät konnte nicht aufgebaut werden";
            throw;
        }
        catch (RequestException re) when (re.CustomRequestError == CustomRequestError.NameResolutionError)
        {
            MainContext.Instance.ErrorText = "Der Netzwerkname des Gerätes konnte nicht ermittelt werden";
            throw;
        }
        catch (RequestException)
        {
            MainContext.Instance.ErrorText = "Die Einträge konnten nicht vom entfernten Gerät geholt werden";
            throw;
        }
    }
}

static partial class Extensions
{
    public static CsTools.HttpRequest.Settings PutFile(this Stream streamToPost, IpAndPath ipAndPath, string name, DateTime? lastWrite)
        => DefaultSettings with
        {
            Method = HttpMethod.Put,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/putfile/{ipAndPath.Path.CombineRemotePath(name)}",
            Timeout = 100_000_000,
            AddContent = () => new StreamContent(streamToPost, 15000)
                                    .SideEffectIf(lastWrite.HasValue, n => n.Headers
                                                        .TryAddWithoutValidation(
                                                            "x-file-date",
                                                            new DateTimeOffset(lastWrite!.Value).ToUnixTimeMilliseconds().ToString()))
        };
}