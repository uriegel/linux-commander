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
    string? _CurrentPath;

    public int CurrentFiles
    {
        get => _CurrentFiles;
        set
        {
            _CurrentFiles = value;
            OnChanged(nameof(CurrentFiles));
        }
    }
    int _CurrentFiles;
    
    public int CurrentDirectories
    {
        get => _CurrentDirectories;
        set
        {
            _CurrentDirectories = value;
            OnChanged(nameof(CurrentDirectories));
        }
    }
    int _CurrentDirectories;

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}