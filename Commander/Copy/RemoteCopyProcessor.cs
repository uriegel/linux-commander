using Commander.Controllers;
using Commander.UI;
using CsTools;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using GtkDotNet;

using static CsTools.HttpRequest.Core;

namespace Commander.Copy;

class RemoteCopyProcessor(string sourcePath, string targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems, bool move)
    : CopyProcessor(sourcePath, targetPath, selectedItemsType, selectedItems, move)
{
    protected override DirectoryItem? ValidateFile(DirectoryItem item, string subPath, string path) => item;

    protected override async Task CopyItem(CopyItem item, byte[] buffer, CancellationToken cancellation)
    {
        try
        {
            var newFileName = TargetPath.AppendPath(item.Source.Name);
            var tmpNewFileName = TargetPath.AppendPath(item.Source.Name + TMP_POSTFIX);
            long? lastWrite = null;
            var source = SourcePath.CombineRemotePath(item.Source.Name);
            var msg = await Request
                .Run(SourcePath.GetIpAndPath().GetFile(item.Source.Name), true)
                .HttpGetOrThrowAsync();
            var len = msg.Content.Headers.ContentLength;
            try
            {
                using var target =
                    File
                        .Create(tmpNewFileName.EnsureFileDirectoryExists())
                        .WithProgress((t, c) => ProgressContext.Instance.SetProgress(len ?? t, c));
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
            if (lastWrite.HasValue)
                File.SetLastWriteTime(tmpNewFileName, lastWrite.Value.FromUnixTime());

            await Gtk.Dispatch(() =>
            {
                using var gsf = GFile.New(SourcePath.AppendPath(item.Source.Name));
                using var gtf = GFile.New(tmpNewFileName);
                gsf.CopyAttributes(gtf, FileCopyFlags.Overwrite);
            });
            File.Move(tmpNewFileName, newFileName, true);
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
    public static CsTools.HttpRequest.Settings GetFile(this IpAndPath ipAndPath, string name)
        => DefaultSettings with
        {
            Method = HttpMethod.Get,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/downloadfile/{ipAndPath.Path.CombineRemotePath(name)}",
        };
}