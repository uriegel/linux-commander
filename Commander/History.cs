namespace Commander;

class History
{
    public void Set(string path)
    {
        if (history.Count == 0 || history[^1] != path)
        {
            history.Add(path);
            position = history.Count - 1;
        }
    }

    public string? Get(bool reverse)
        => reverse
            ? GetNext()
            : GetPrevious();

    string? GetPrevious()
        => position > 0
            ? history[--position]
            : null;

    string? GetNext()
        => position < history.Count - 1
            ? history[++position]
            : null;

    int position;
    readonly List<string> history = [];
}