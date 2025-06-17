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
            GetController(data.Id).ChangePath(data.Path);
            var response = new ChangePathResult("der Pfad");
            await request.SendJsonAsync(response);
        }
        return true;
    }
}
record ChangePathRequest(
    string Id,
    string Path
);
record ChangePathResult(string Path);