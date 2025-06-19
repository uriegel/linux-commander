using WebServerLight;

using static CsTools.Functional.Memoization;
using static CsTools.ProcessCmd;
using static Commander.Controllers.FolderController;
using CsTools.Extensions;
using Commander.Settings;
using CsTools;
using System.Text;

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
        var iconfile = (await RunAsync("python3", $"{script} {request.SubPath}")).Trim();
        using var file = File.OpenRead(iconfile);
        var stream = iconfile.Contains("symbolic") ? WithSymbolicTheme(file) : file as Stream;
        await request.SendAsync(stream, stream.Length, iconfile.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ? "image/svg+xml" : "image/png");
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

    static MemoryStream WithSymbolicTheme(FileStream input)
    {
        var text = new StreamReader(input).ReadToEnd();
        var pos = 0;
        while (true)
        {
            var fillPos = text.IndexOf("fill=\"", pos) + 6;
            if (fillPos == -1)
                break;
            var posEnd = text.IndexOf('\"', fillPos + 1);
            var newText = string.Concat(text.AsSpan(0, fillPos), "#888", text.AsSpan(posEnd));
            text = newText;
            pos = fillPos;
            break;
        }
        return new MemoryStream(Encoding.UTF8.GetBytes(text));
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

