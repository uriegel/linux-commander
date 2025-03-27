using System.ComponentModel;

namespace Commander.DataContexts;

class FolderContext : INotifyPropertyChanged
{
    public string CurrentPath
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(CurrentPath));
        }
    } = "";

    public int CurrentFiles
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(CurrentFiles));
        }
    }
    
    public int CurrentDirectories
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(CurrentDirectories));
        }
    }

     public string? SelectedPath
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(SelectedPath));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}