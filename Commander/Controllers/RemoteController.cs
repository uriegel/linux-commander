using Commander.UI;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using static System.Console;

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

    public static string CheckParent(this string path)
        => path.EndsWith("..")
            ? path.SubstringUntilLast('/').SubstringUntilLast('/')
            : path;
}

record RemoteItem(
    string Name,
    long Size,
    bool IsDirectory,
    bool IsHidden,
    long Time
);

record IpAndPath(string Ip, string Path);    