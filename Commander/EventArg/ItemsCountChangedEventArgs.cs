namespace Commander.EventArg;

public class ItemsCountChangedEventArgs(int directories, int items) : EventArgs
{
    public int Directories { get => directories; }
    public int Items { get => items; }
}