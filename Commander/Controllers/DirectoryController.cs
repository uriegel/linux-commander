namespace Commander.Controllers;

class DirectoryController : Controller
{
    public override string Id { get; } = "DIRECTORY";

    public override async Task<ChangePathResult> ChangePathAsync(string path)
    {
        return new RootResult(Id, 0, 0, []);
    }
}