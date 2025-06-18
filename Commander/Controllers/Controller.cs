namespace Commander.Controllers;

abstract class Controller
{
    public abstract string Id { get; }
    public abstract Task<ChangePathResult> ChangePathAsync(string path);

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