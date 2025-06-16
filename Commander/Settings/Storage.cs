using CsTools;
using CsTools.Extensions;

namespace Commander.Settings;

static class Storage
{
    public static Value Retrieve()
        => Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("settings.json")
            .ReadAllTextFromFilePath()
            ?.Deserialize<Value>(Json.Defaults)
//            ?? new("root", "root", [], [], null);
                ?? new("root", "root");

    public static void SaveLeftPath(string path)
        => Save(Retrieve() with { LeftPath = path });
    public static void SaveRightPath(string path)
        => Save(Retrieve() with { RightPath = path });

    // public static void SaveFavorite(FavoritesItem favorite)
    // {
    //     var val = Retrieve();
    //     Save(val with { Favorites = val.Favorites != null ? [.. val.Favorites, favorite] : [ favorite ]});    
    // }
    
    // public static void SaveRemote(RemotesItem remote)
    // {
    //     var val = Retrieve();
    //     Save(val with { Remotes = val.Remotes != null ? [.. val.Remotes, remote] : [ remote ]});    
    // }

    // public static void SaveExtendedRename(ExtendedRenameData data)
    // {
    //     var val = Retrieve();
    //     Save(val with { ExtendedRenameData = data });    
    // }

    // public static void DeleteFavorite(FavoritesItem favorite)
    // {
    //     var val = Retrieve();
    //     Save(val with { Favorites = val.Favorites?.Where(n => n != favorite).ToArray() });
    // }

    // public static void ChangeFavorite(FavoritesItem favorite, string newName)
    // {
    //     var val = Retrieve();
    //     if (val.Favorites != null)
    //     {
    //         var pos = Array.IndexOf(val.Favorites, favorite);
    //         val.Favorites[pos] = val.Favorites[pos] with { Name = newName };
    //         Save(val with { Favorites = val.Favorites });    
    //     }
    // }

    // public static void ChangeRemote(RemotesItem remote, string newName)
    // {
    //     var val = Retrieve();
    //     if (val.Remotes != null)
    //     {
    //         var pos = Array.IndexOf(val.Remotes, remote);
    //         val.Remotes[pos] = val.Remotes[pos] with { Name = newName };
    //         Save(val with { Remotes = val.Remotes });    
    //     }
    // }

    // public static void DeleteRemote(RemotesItem remote)
    // {
    //     var val = Retrieve();
    //     Save(val with { Remotes = val.Remotes?.Where(n => n != remote).ToArray() });
    // }

    static void Save(Value value)
        => Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("settings.json")
            .WriteAllTextToFilePath(value.Serialize(Json.Defaults));
}

record Value(
    string LeftPath,
    string RightPath);
    // FavoritesItem[]? Favorites,
    // RemotesItem[]? Remotes,
    // ExtendedRenameData? ExtendedRenameData);