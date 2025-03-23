using System.ComponentModel;

namespace Commander.DataContexts;

class MainContext : INotifyPropertyChanged
{
    public static MainContext Instance = new();
    public string? CurrentPath
    {
        get => _CurrentPath;
        set
        {
            _CurrentPath = value;
            OnChanged(nameof(CurrentPath));
        }
    }
    string? _CurrentPath = "Das soll er sein";

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}