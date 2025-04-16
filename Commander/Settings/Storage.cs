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
            ?? new("root", "root", []);

    public static void SaveLeftPath(string path)
        => Save(Retrieve() with { LeftPath = path });
    public static void SaveRightPath(string path)
        => Save(Retrieve() with { RightPath = path });

    public static void SaveFavorite(Favorite favorites)
    {
        var val = Retrieve();
        Save(val with { Favorites = [.. val.Favorites, favorites] });    
    }
    

    static void Save(Value value)
        => Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("settings.json")
            .WriteAllTextToFilePath(value.Serialize(Json.Defaults));
}

record Favorite(string Name, string Path);
record Value(
    string LeftPath,
    string RightPath,
    Favorite[] Favorites);