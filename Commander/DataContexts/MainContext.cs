using System.ComponentModel;
using Commander.Enums;

namespace Commander.DataContexts;

class MainContext : INotifyPropertyChanged
{
    public static MainContext Instance = new();

    public string? SelectedPath
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(SelectedPath));
                OnChanged(nameof(StatusChoice));
            }
        }
    }

    public string? Restriction
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(Restriction));
                OnChanged(nameof(StatusChoice));
            }
        }
    }
    
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
                OnChanged(nameof(StatusChoice));
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

    public string? ErrorText
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(ErrorText));
                if (value != null)
                {
                    Reset();
                    async void Reset()
                    {
                        await Task.Delay(3000);
                        ErrorText = null;
                    }
                }
            }
        }
    }

    public string? InfoText
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(InfoText));
            }
        }
    }
    
    public StatusChoice StatusChoice
    {
        get => Restriction?.Trim()?.Length > 0
                ? StatusChoice.Restriction
                : BackgroundAction != BackgroundAction.None
                ? StatusChoice.BackgroundAction
                : SelectedFiles > 0
                ? StatusChoice.SelectedItems
                : StatusChoice.Status;
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
                OnChanged(nameof(StatusChoice));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void ChangeFolderContext(FolderContext? folderContext)
    {
        if (folderContext != null)
        {
            if (this.folderContext != null)
                this.folderContext.PropertyChanged -= FolderContextPropertyChanged;
            this.folderContext = folderContext;
            this.folderContext.PropertyChanged += FolderContextPropertyChanged;
            CurrentDirectories = folderContext.CurrentDirectories;
            CurrentFiles = folderContext.CurrentFiles;
            SelectedPath = folderContext.SelectedPath;
            ExifData = folderContext.ExifData;
            BackgroundAction = folderContext.BackgroundAction;
            SelectedFiles = folderContext.SelectedFiles;
        }
    }
    void FolderContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (folderContext != null)
            switch (e.PropertyName)
            {
                case nameof(CurrentDirectories):
                    CurrentDirectories = folderContext.CurrentDirectories;
                    break;
                case nameof(CurrentFiles):
                    CurrentFiles = folderContext.CurrentFiles;
                    break;
                case nameof(SelectedPath):
                    SelectedPath = folderContext.SelectedPath;
                    break;
                case nameof(ExifData):
                    ExifData = folderContext.ExifData;
                    break;
                case nameof(SelectedFiles):
                    SelectedFiles = folderContext.SelectedFiles;
                    break;
                case nameof(BackgroundAction):
                    BackgroundAction = folderContext.BackgroundAction;
                    break;
            }
    }

    FolderContext? folderContext;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}