namespace Commander.Controllers;

class DirectoryController : Controller
{
    public override string Id { get; } = "DIRECTORY";

    public override async Task<ChangePathResult> ChangePathAsync(string path)
    {
        // TODO Set the right columns
        return new RootResult(Id, 0, 0, []);
    }
}