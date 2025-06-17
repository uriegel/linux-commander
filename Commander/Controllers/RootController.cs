namespace Commander.Controllers;

class RootController : IController
{
    public override string Id { get; } = "ROOT";

    public override ChangePathResult ChangePath(string path)
    {
        return new(CheckInitial() ? Id : null);
        // TODO get root items
    }
}
