using System.Text;
using CsTools;
using CsTools.Extensions;
using WebServerLight;
using Commander.Settings;

using static CsTools.Functional.Memoization;
using static CsTools.ProcessCmd;
using static Commander.Controllers.FolderController;
using Commander.Controllers;
using Commander.Copy;

namespace Commander;

static class Requests
{
    public static async Task<bool> Process(IRequest request)
        => request.SubPath switch
        {
            "changepath" => await ChangePath(request),
            "getextended" => await GetExtended(request),
            "preparecopy" => await PrepareCopy(request),
            "copy" => await Copy(request),
            "delete" => await Delete(request),
            "createfolder" => await CreateFolder(request),
            "rename" => await Rename(request),
            "onenter" => await OnEnter(request),
            "onextendedrename" => await OnExtendedRename(request),
            _ => false
        };

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

    public static async Task<bool> GetFile(IRequest request)
    {
        if (request.SubPath == null)
            return false;
        using var pic = File.OpenRead("/" + request.SubPath);
        if (pic != null)
        {
            await request.SendAsync(pic, pic.Length, MimeType.Get(request.SubPath.GetFileExtension()) ?? MimeTypes.ImageJpeg);
            return true;
        }
        else
            return false;
    }

    public static async Task<bool> GetTrack(IRequest request)
    {
        if (request.SubPath == null)
            return false;
        var track = TrackInfo.Get("/" + request.SubPath);
        await request.SendJsonAsync(track);
        return true;
    }
    
    public static void WebSocket(IWebSocket webSocket)
        => Requests.webSocket = webSocket;

    public static async void SendMenuCommand(string id)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("cmd", new(id), null, null, null));    
    }

    public static async void SendMenuCheck(string id, bool check)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("cmdtoggle", null, new(id, check), null, null)); 
    }

    public static async void SendStatusBarInfo(string id, int requestId, string? text)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("status", null, null, new(id, requestId, text), null)); 
    }

    public static async void SendExifInfo(string id, int requestId, DirectoryItem[] items)
    {
        try
        {
            if (webSocket != null)
                await webSocket.SendJson(new WebSocketMsg("exifinfo", null, null, null, new(id, requestId, items)));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending exif info: {ex}");
        }
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
        
    static Task<bool> ChangePath(IRequest request)
        => Request<ChangePathRequest, ChangePathResult>(request, n =>
        {
            var path = n.Mount ? n.Path.Mount() : n.Path;
            DetectController(n.Id, path);
            return GetController(n.Id).ChangePathAsync(path, n.ShowHidden);
        });

    static Task<bool> GetExtended(IRequest request)
        => Request<GetExtendedRequest, GetExtendedResult>(request, n => GetController(n.FolderId).GetExtended(n.Id));

    static Task<bool> PrepareCopy(IRequest request)
        => Request<PrepareCopyRequest, PrepareCopyResult>(request, n => GetController(n.Id).PrepareCopy(n));
    
    static Task<bool> Copy(IRequest request)
        => Request<CopyRequest, CopyResult>(request, n => GetController(n.Id).Copy(n));

    static Task<bool> Delete(IRequest request)
        => Request<DeleteRequest, DeleteResult>(request, n => GetController(n.Id).Delete(n));

    static Task<bool> CreateFolder(IRequest request)
        => Request<CreateFolderRequest, CreateFolderResult>(request, n => GetController(n.Id).CreateFolder(n));

    static Task<bool> Rename(IRequest request)
        => Request<RenameRequest, RenameResult>(request, n => GetController(n.Id).Rename(n));

    static Task<bool> OnEnter(IRequest request)
        => Request<OnEnterRequest, OnEnterResult>(request, n => GetController(n.Id).OnEnter(n));

    static Task<bool> OnExtendedRename(IRequest request)
        => Request<OnExtendedRenameRequest, OnExtendedRenameResult>(request, n => GetController(n.Id).OnExtendedRename(n));
    
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

    static async Task<bool> Request<TRequest, TResponse>(IRequest request, Func<TRequest, Task<TResponse>> onRequest)
        where TResponse: class
    {
        var data = await request.DeserializeAsync<TRequest>();
        if (data != null)
        {
            var response = await onRequest(data);
            await request.SendJsonAsync(response, response.GetType());
        }
        return true;
    }

    static IWebSocket? webSocket;
}
record ChangePathRequest(
    string Id,
    string Path,
    bool Mount,
    bool ShowHidden

);
record ChangePathResult(
    bool? Cancelled,
    int Id,
    string? Controller,
    string Path,
    int DirCount,
    int FileCount
);

record GetExtendedRequest(int Id, string FolderId);
record GetExtendedResult();

record PrepareCopyRequest(
    string Id,
    string Path,
    string TargetPath,
    bool Move,
    DirectoryItem[] Items
);

record PrepareCopyResult(
    SelectedItemsType SelectedItemsType,
    long TotalSize,
    CopyItem[] Conflicts
);

record CopyRequest(
    string Id,
    bool Cancelled,
    bool NotOverwrite
);

record CopyResult(
    bool Cancelled
);

record DeleteRequest(
    string Id,
    string Path,
    DirectoryItem[] Items
);

record DeleteResult(bool Success);

record CreateFolderRequest(
    string Id,
    string Path,
    string Name
);

record CreateFolderResult(bool Success);

record RenameRequest(
    string Id,
    string Path,
    string Name,
    string NewName,
    bool Copy
);

record RenameResult(bool Success);

record OnEnterRequest(
    string Id,
    string Path,
    string Name
);

record OnEnterResult(bool Success);

record OnExtendedRenameRequest(
    string Id,
    string Path,
    DirectoryItem[] Items
);
record OnExtendedRenameResult(bool Success);

record ViewItem(
    string Name,
    long? Size,
    bool? IsParent,
    bool? IsDirectory
);

record WebSocketMsg(
    string Method,
    CmdMsg? CmdMsg,
    CmdToggleMsg? CmdToggleMsg,
    StatusMsg? StatusMsg,
    ExifMsg? ExifMsg);

record CmdMsg(string Cmd);
record CmdToggleMsg(string Cmd, bool Checked);
record StatusMsg(
    string FolderId,
    int RequestId,
    string? Text);
record ExifMsg(
    string FolderId,
    int RequestId,
    DirectoryItem[] Items);

// export interface FolderViewItem extends SelectableItem {
//     // FileSystem item
//     // Remotes item
//     ipAddress?:   string
//     isAndroid?:   boolean
//     isNew?: boolean
//     // Favorites
//     path?: string | null
// }

