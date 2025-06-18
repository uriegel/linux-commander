using WebServerLight;

using static CsTools.Functional.Memoization;
using static CsTools.ProcessCmd;
using static Commander.Controllers.FolderController;
using CsTools.Extensions;
using Commander.Settings;
using CsTools;

namespace Commander;

static class Requests
{
    public static async Task<bool> Process(IRequest request)
    {
        return request.SubPath switch
        {
            "changepath" => await ChangePath(request),
            _ => false
        };
    }

    public static async Task<bool> GetIconFromName(IRequest request)
    {
        var script = IconFromNameScript();
        var iconfile = (await RunAsync("python", $"{script} {request.SubPath}")).Trim();
        using var file = File.OpenRead(iconfile);
        await request.SendAsync(file, file.Length, iconfile.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ? "image/svg+xml" : "image/png");
        return true;
    }

    static async Task<bool> ChangePath(IRequest request)
    {
        var data = await request.DeserializeAsync<ChangePathRequest>();
        if (data != null)
        {
            DetectController(data.Id, data.Path);
            var response = await GetController(data.Id).ChangePathAsync(data.Path);
            await request.SendJsonAsync(response, response.GetType());
        }
        return true;
    }

    static Func<string> IconFromNameScript { get; } = Memoize(IconFromNameScriptInit);

    static string IconFromNameScriptInit()
    {
        var res = Resources.Get("iconfromname");
        var filename = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("iconfromname.py");
        using var file = File.Create(filename);
        res?.CopyTo(file);
        return filename;
    }
}
record ChangePathRequest(
    string Id,
    string Path
);
record ChangePathResult(
    string? Controller,
    int DirCount,
    int FileCount
);

record ViewItem(
    string Name,
    long? Size,
    bool? IsParent,
    bool? IsDirectory
);

// export interface FolderViewItem extends SelectableItem {
//     // FileSystem item
//     iconPath?:    string
//     time?:        string
//     // exifData?:    ExifData
//     isHidden?:    boolean
//     // Remotes item
//     ipAddress?:   string
//     isAndroid?:   boolean
//     isNew?: boolean
//     // ExtendedRename
//     newName?:     string|null
//     // Favorites
//     path?: string | null
// }

