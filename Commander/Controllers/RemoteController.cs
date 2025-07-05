using Commander.Copy;
using Commander.UI;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;

using static System.Console;
using static CsTools.HttpRequest.Core;

namespace Commander.Controllers;

class RemoteController(string folderId) : Controller(folderId)
{
    public override string Id { get; } = "REMOTE";

    public override async Task<ChangePathResult> ChangePathAsync(string path, bool showHidden)
    {
        var changePathId = Interlocked.Increment(ref ChangePathSeed);
        try
        {
            var pathToSet = path.CheckParent();
            var cancellation = Cancellations.ChangePathCancellation(FolderId);
            var items = (await pathToSet
                .GetIpAndPath()
                .Pipe(ipPath =>
                    ipPath
                        .GetRequest()
                        .Get<RemoteItem[]>($"getfiles{ipPath.Path}", true)
                        .Select(n => n
                            .OrderByDescending(n => n.IsDirectory)
                            .ThenBy(n => n.Name)
                            .Where(n => showHidden || !n.IsHidden)
                            .Select(n => new DirectoryItem(
                                    n.Name,
                                    n.Size,
                                    n.IsDirectory,
                                    false,
                                    n.IsHidden,
                                    n.Time.FromUnixTime().Pipe(n => n.Year == 1970 ? (DateTime?)null : n),
                                    null))))
                .HttpGetOrThrowAsync())
                .ToArray();
            cancellation.ThrowIfCancellationRequested();
            var dirs = items.Count(n => n.IsDirectory);
            var files = items.Count(n => !n.IsDirectory);
            return new GetFilesResult(null, changePathId, CheckInitial() ? Id : null, pathToSet, dirs, files, [new DirectoryItem("..", 0, true, true, false, null, null), ..items]);
        }
        catch (RequestException re) when (re.CustomRequestError == CustomRequestError.ConnectionError)
        {
            OnError(re);
            MainContext.Instance.ErrorText = "Die Verbindung zum Gerät konnte nicht aufgebaut werden";
            return new ChangePathResult(true, changePathId, null, "", 0, 0);
        }
        catch (RequestException re) when (re.CustomRequestError == CustomRequestError.NameResolutionError)
        {
            OnError(re);
            MainContext.Instance.ErrorText = "Der Netzwerkname des Gerätes konnte nicht ermittelt werden";
            return new ChangePathResult(true, changePathId, null, "", 0, 0);
        }
        catch (OperationCanceledException)
        {
            return new ChangePathResult(true, changePathId, null, "", 0, 0);
        }
        catch (Exception e)
        {
            OnError(e);
            MainContext.Instance.ErrorText = "Ordner konnte nicht gewechselt werden";
            return new ChangePathResult(true, changePathId, null, "", 0, 0);
        }

        static void OnError(Exception e) => Error.WriteLine($"Konnte Pfad nicht ändern: {e}");
    }

    public override Task<PrepareCopyResult> PrepareCopy(PrepareCopyRequest data)
    {
        var items = data
                        .Items
                        .Where(n => !n.IsDirectory)
                        .ToArray();
        if ((data.TargetPath.StartsWith('/') != true && data.TargetPath?.StartsWith("remote/") != true)
        || string.Compare(data.Path, data.TargetPath, StringComparison.CurrentCultureIgnoreCase) == 0
        || items.Length == 0)
            return new PrepareCopyResult(SelectedItemsType.None, 0, []).ToAsync();
        var copyProcessor = new RemoteCopyProcessor(data.Path, data.TargetPath, GetSelectedItemsType(items), items, data.Move);
        return Task.Run(copyProcessor.PrepareCopy);
    }

    public override Task<CopyResult> Copy(CopyRequest copyRequest) => CopyProcessor.Current?.Copy(copyRequest) ?? new CopyResult(true).ToAsync();

    public override async Task<DeleteResult> Delete(DeleteRequest deleteRequest)
    {
        try
        {
            var cancellation = ProgressContext.Instance.Start(deleteRequest.Id, "Löschen", deleteRequest.Items.Length, deleteRequest.Items.Length, true);
            var index = 0;
            foreach (var item in deleteRequest.Items)
            {
                if (cancellation.IsCancellationRequested)
                    throw new TaskCanceledException();
                ProgressContext.Instance.SetNewFileProgress(item.Name, 1, ++index);
                await Request
                        .Run(deleteRequest.Path
                        .CombineRemotePath(item.Name).GetIpAndPath()
                        .DeleteItem())
                        .HttpGetOrThrowAsync();
                ProgressContext.Instance.SetProgress(1, 1);
            }
        }
        finally
        {
            ProgressContext.Instance.Stop();
        }

        return new(true);
    }

    public override async Task<CreateFolderResult> CreateFolder(CreateFolderRequest createFolderRequest)
    {
        try
        {
            await Request
                .Run(createFolderRequest.Path.CombineRemotePath(createFolderRequest.Name).GetIpAndPath().PostCreateDirectory(), true)
                .HttpGetOrThrowAsync();
            return new CreateFolderResult(true);
        }
        catch (RequestException)
        {
            MainContext.Instance.ErrorText = "Der  Ordner konnte nicht angelegt werden";
            return new CreateFolderResult(false);
        }
    }

    static SelectedItemsType GetSelectedItemsType(DirectoryItem[] items)
    {
        var files = items.Count(n => !n.IsDirectory);
        return files > 1
            ? SelectedItemsType.Files
            : files == 1
            ? SelectedItemsType.File
            : SelectedItemsType.None;
    }


    public static int ChangePathSeed = 0;
}

static partial class Extensions
{
    public static IpAndPath GetIpAndPath(this string url)
        => new(url.StringBetween('/', '/'),
            url[7..].Contains('/')
            ? "/" + url.SubstringAfter('/').SubstringAfter('/')
            : "");

    public static JsonRequest GetRequest(this IpAndPath ipAndPath)
        => new($"http://{ipAndPath.Ip}:8080");

    public static string CombineRemotePath(this string path, string subPath)
        => path.EndsWith('/')
            ? subPath.StartsWith('/')
                ? path + subPath[1..]
                : path + subPath
            : subPath.StartsWith('/')
                ? path + subPath
                : path + '/' + subPath;

    public static string CheckParent(this string path)
        => path.EndsWith("..")
            ? path.SubstringUntilLast('/').SubstringUntilLast('/')
            : path;

    public static CsTools.HttpRequest.Settings PostCreateDirectory(this IpAndPath ipAndPath)
        => DefaultSettings with
        {
            Method = HttpMethod.Post,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/createdirectory/{ipAndPath.Path}",
        };    

    public static CsTools.HttpRequest.Settings DeleteItem(this IpAndPath ipAndPath) 
        => DefaultSettings with
        {
            Method = HttpMethod.Delete,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/deletefile/{ipAndPath.Path}",
        };    
}

record RemoteItem(
    string Name,
    long Size,
    bool IsDirectory,
    bool IsHidden,
    long Time
);

record IpAndPath(string Ip, string Path);    