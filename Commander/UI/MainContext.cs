using System.ComponentModel;

namespace Commander.UI;

class MainContext : INotifyPropertyChanged
{
    public static MainContext Instance = new();

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

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}