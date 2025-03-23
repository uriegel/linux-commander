namespace Commander.EventArg;

public class PosChangedEventArgs(string? currentPath) : EventArgs
{
    public string? CurrentPath { get => currentPath; }
}