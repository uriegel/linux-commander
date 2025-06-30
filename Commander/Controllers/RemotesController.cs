
using CsTools.Extensions;

namespace Commander.Controllers;

class RemotesController(string folderId) : Controller(folderId)
{
    public override string Id { get; } = "REMOTES";

    public override Task<ChangePathResult> ChangePathAsync(string path, bool showHidden)
        => new ChangePathResult(null, 0, CheckInitial() ? Id : null, "remotes", 0, 0).ToAsync();
}