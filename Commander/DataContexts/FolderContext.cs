using System.ComponentModel;

namespace Commander.DataContexts;

class FolderContext : INotifyPropertyChanged
{
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

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}