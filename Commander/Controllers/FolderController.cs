using System.Collections.Concurrent;

namespace Commander.Controllers;

static class FolderController
{
    public static IController DetectController(string id, string path)
        => controllers.AddOrUpdate(
            id,
            _ => CreateController(path),
            (_, controller) => SetController(controller, () => CreateController(path)));

    public static IController GetController(string id) => controllers[id];

    static IController SetController<T>(IController current, Func<T> factory)
        where T : IController
    {
        if (current is not T)
            current = factory();
        return current;
    }

    static IController CreateController(string path)
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

    static readonly ConcurrentDictionary<string, IController> controllers = new();
}