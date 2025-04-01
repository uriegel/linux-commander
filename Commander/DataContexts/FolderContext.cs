using System.ComponentModel;
using Commander.Enums;
using Commander.Settings;

namespace Commander.DataContexts;

class FolderContext : INotifyPropertyChanged
{
    public bool IsLeft { get; set; } 
    public string CurrentPath
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                if (IsLeft)
                    Storage.SaveLeftPath(value);
                else
                    Storage.SaveRightPath(value);
                OnChanged(nameof(CurrentPath));
            }
        }
    } = "";

    public int CurrentFiles
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(CurrentFiles));
            }
        }
    }
    
    public int CurrentDirectories
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(CurrentDirectories));
            }
        }
    }

    public int SelectedFiles
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(SelectedFiles));
            }
        }
    }

    public string? SelectedPath
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(SelectedPath));
            }
        }
    }

    public ExifData? ExifData
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(ExifData));
            }
        }
    }

    public BackgroundAction BackgroundAction
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(BackgroundAction));
            }
        }
    }

    public bool IsEditing { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}