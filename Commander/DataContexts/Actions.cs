using System.ComponentModel;

namespace Commander.DataContexts;

class Actions : INotifyPropertyChanged
{
    public static Actions Instance = new();

    public bool ShowHidden
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(ShowHidden));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}