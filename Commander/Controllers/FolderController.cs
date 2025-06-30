using System.Collections.Concurrent;

namespace Commander.Controllers;

static class FolderController
{
    public static Controller DetectController(string id, string path)
        => controllers.AddOrUpdate(
            id,
            _ => CreateController(id, path),
            (_, controller) => SetController(controller, path, () => CreateController(id, path)));

    public static Controller GetController(string id) => controllers[id];

    static Controller SetController<T>(Controller current, string path, Func<T> factory)
        where T : Controller
    {
        if (current.GetType() != GetControllerType(path))
            current = factory();
        return current;
    }

    static Type GetControllerType(string path)
        => path switch
        {
            "root" => typeof(RootController),
            "/.." => typeof(RootController),
            "" => typeof(RootController),
            "remotes" => typeof(RemotesController),
            //_ when path.StartsWith("remote") => SetController(() => new RemoteController(folderView)),
            "fav" => typeof(FavoritesController),
            _ => typeof(DirectoryController)
        };


    static Controller CreateController(string folderId, string path)
        => path switch
        {
            "root" => new RootController(folderId),
            "/.." => new RootController(folderId),
            "" => new RootController(folderId),
            "remotes" => new RemotesController(folderId),
            //_ when path.StartsWith("remote") => SetController(() => new RemoteController(folderView)),
            "fav" => new FavoritesController(folderId),
            _ => new DirectoryController(folderId)
        };

    static readonly ConcurrentDictionary<string, Controller> controllers = new();
}