using System.Collections.Concurrent;

namespace Commander.Controllers;

static class FolderController
{
    public static Controller DetectController(string id, string path)
        => controllers.AddOrUpdate(
            id,
            _ => CreateController(path),
            (_, controller) => SetController(controller, () => CreateController(path)));

    public static Controller GetController(string id) => controllers[id];

    static Controller SetController<T>(Controller current, Func<T> factory)
        where T : Controller
    {
        if (current is not T)
            current = factory();
        return current;
    }

    static Controller CreateController(string path)
        => path switch
        {
            //"fav" => new FavoritesController(folderView),
            //"remotes" => SetController(() => new RemotesController(folderView)),
            "root" => new RootController(),
            "" => new RootController(),
            //_ when path.StartsWith("remote") => SetController(() => new RemoteController(folderView)),
            //_ => new DirectoryController()
            _ => new RootController()
        };

    static readonly ConcurrentDictionary<string, Controller> controllers = new();
}