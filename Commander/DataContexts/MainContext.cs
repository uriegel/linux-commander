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

    public string? CurrentFiles
    {
        get => _CurrentFiles;
        set
        {
            _CurrentFiles = value;
            OnChanged(nameof(CurrentFiles));
        }
    }
    string? _CurrentFiles;
    
    public string? CurrentDirectories
    {
        get => _CurrentDirectories;
        set
        {
            _CurrentDirectories = value;
            OnChanged(nameof(CurrentDirectories));
        }
    }
    string? _CurrentDirectories;

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}