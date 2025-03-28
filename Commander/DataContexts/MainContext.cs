using System.ComponentModel;

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
            }
    }

    FolderContext? folderContext;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}