using System.ComponentModel;

namespace Commander.DataContexts;

class Actions : INotifyPropertyChanged
{
    public static Actions Instance = new();
    public bool ShowHidden
    {
        get => _ShowHidden;
        set
        {
            _ShowHidden = value;
            OnChanged(nameof(ShowHidden));
        }
    }
    bool _ShowHidden;

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}