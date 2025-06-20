using System.Text;
using CsTools;
using CsTools.Extensions;
using WebServerLight;
using Commander.Settings;

using static CsTools.Functional.Memoization;
using static CsTools.ProcessCmd;
using static Commander.Controllers.FolderController;

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
        var iconfile = await IconFromName(request.SubPath ?? "emtpy");
        using var file = File.OpenRead(iconfile!);
        var stream = iconfile?.Contains("symbolic") == true ? WithSymbolicTheme(file) : file as Stream;
        request.AddResponseHeader("Expires", (DateTime.UtcNow + TimeSpan.FromHours(1)).ToString("r"));
        await request.SendAsync(stream, stream.Length, iconfile?.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) == true ? "image/svg+xml" : "image/png");
        return true;
    }

    public static async Task<bool> GetIconFromExtension(IRequest request)
    {
        var iconfile = await IconFromExtension(request.SubPath ?? "xxx");
        using var file = File.OpenRead(iconfile!);
        var stream = iconfile?.Contains("symbolic") == true ? WithSymbolicTheme(file) : file as Stream;
        request.AddResponseHeader("Expires", (DateTime.UtcNow + TimeSpan.FromHours(1)).ToString("r"));
        await request.SendAsync(stream, stream.Length, iconfile?.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) == true ? "image/svg+xml" : "image/png");
        return true;
    }
    
    public static void WebSocket(IWebSocket webSocket)
        => Requests.webSocket = webSocket;

    public static async void SendMenuCommand(string id)
    {
        if (webSocket != null)
            await webSocket.SendJson(new MenuCommand(id));    
    }

    static Func<string, Task<string?>> IconFromName { get; } = MemoizeAsync<string>(IconFromNameInit, false);
    static Func<string, Task<string?>> IconFromExtension { get; } = MemoizeAsync<string>(IconFromExtensionInit, false);

    static async Task<string?> IconFromNameInit(string iconName, string? oldValue)
    {
        var script = IconFromNameScript();
        return (await RunAsync("python3", $"{script} {iconName}")).Trim();
    }

    static async Task<string?> IconFromExtensionInit(string iconName, string? oldValue)
    {
        var script = IconFromExtensionScript();
        return (await RunAsync("python3", $"{script} {iconName.WhiteSpaceToNull() ?? "xxx"}")).Trim();
    }
        
    static async Task<bool> ChangePath(IRequest request)
    {
        var data = await request.DeserializeAsync<ChangePathRequest>();
        if (data != null)
        {
            var path = data.Mount ? data.Path.Mount() : data.Path;
            DetectController(data.Id, path);
            var response = await GetController(data.Id).ChangePathAsync(path);
            await request.SendJsonAsync(response, response.GetType());
        }
        return true;
    }

    static Func<string> IconFromNameScript { get; } = Memoize(IconFromNameScriptInit);
    static Func<string> IconFromExtensionScript { get; } = Memoize(IconFromExtensionScriptInit);

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

    static string IconFromExtensionScriptInit()
    {
        var res = Resources.Get("iconfromextension");
        var filename = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("iconfromextension.py");
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

    static IWebSocket? webSocket;
}
record ChangePathRequest(
    string Id,
    string Path,
    bool Mount

);
record ChangePathResult(
    string? Controller,
    string Path,
    int DirCount,
    int FileCount
);

record ViewItem(
    string Name,
    long? Size,
    bool? IsParent,
    bool? IsDirectory
);

record MenuCommand(
    string CmdId
);

// export interface FolderViewItem extends SelectableItem {
//     // FileSystem item
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

