namespace Commander.Controllers;

abstract class Controller(string folderId)
{
    public string FolderId { get => folderId; }
    public abstract string Id { get; }
    public abstract Task<ChangePathResult> ChangePathAsync(string path, bool showHidden);

    protected bool CheckInitial()
    {
        if (initial)
        {
            initial = false;
            return true;
        }
        else
            return false;
    }

    bool initial = true;
}