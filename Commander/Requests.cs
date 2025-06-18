using WebServerLight;

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

    static async Task<bool> ChangePath(IRequest request)
    {
        var data = await request.DeserializeAsync<ChangePathRequest>();
        if (data != null)
        {
            DetectController(data.Id, data.Path);
            var response = await GetController(data.Id).ChangePathAsync(data.Path);
            await request.SendJsonAsync(response);
        }
        return true;
    }
}
record ChangePathRequest(
    string Id,
    string Path
);
record ChangePathResult(
    string? Controller,
    int DirCount,
    int FileCount,
    ViewItem[] Items
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

