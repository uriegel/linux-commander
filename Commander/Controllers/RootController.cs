namespace Commander.Controllers;

class RootController : Controller
{
    public override string Id { get; } = "ROOT";

    public override ChangePathResult ChangePath(string path)
    {
        return new(CheckInitial() ? Id : null, 23, 5, [ new RootItem("", "", 89, false, "ja")]);
        // TODO get root items
    }
}


record RootItem(
    string Name,
    string Description,
    long? Size,
    bool IsMounted,
    string MountPoint
)
    : ViewItem(Name, Size, null, null);

