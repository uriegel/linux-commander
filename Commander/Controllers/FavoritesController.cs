
using CsTools.Extensions;

namespace Commander.Controllers;

class FavoritesController(string folderId) : Controller(folderId)
{
    public override string Id { get; } = "FAV";

    public override Task<ChangePathResult> ChangePathAsync(string path, bool showHidden)
        => new ChangePathResult(null, 0, CheckInitial() ? Id : null, "fav", 0, 0).ToAsync();
}