using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Commander.DataContexts;

class CopyProgressContext : INotifyPropertyChanged
{
    public static CopyProgressContext Instance = new();

    public CopyProgress? CopyProgress
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(CopyProgress));
            }
        }
    }

    public void Start(long totalSize)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        CopyProgress = new CopyProgress(
            "",
            totalSize,
            0,
            0,
            0,
            0
        );
    }

    public void SetNewFileProgress(string name, long size)
    {
        var currentSize = Instance.CopyProgress?.PreviousTotalBytes ?? 0;
        Instance.CopyProgress = (Instance.CopyProgress ?? new("", 0, 0, 0, 0, 0)) with
        {
            Name = name,
            TotalBytes = currentSize,
            CurrentMaxBytes = size,
            PreviousTotalBytes = currentSize + size,
            CurrentBytes = 0
        };
    }

    public async void Stop()
    {
        try
        {
            await Task.Delay(5000, cts?.Token ?? default);
            CopyProgress = null;
        }
        catch (TaskCanceledException) { }
    }

    public void SetProgress(long size)
        => progressSubject.OnNext((Instance.CopyProgress ?? new("", 0, 0, 0, 0, 0)) with
        {
            CurrentBytes = size,
        });

    public event PropertyChangedEventHandler? PropertyChanged;

    static CopyProgressContext()
    {
        progressSubject = new();
        progressSubject
            .Sample(TimeSpan.FromMilliseconds(40))
            .Subscribe(value => Instance.CopyProgress = value);
    }

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));

    CancellationTokenSource? cts;

    static readonly Subject<CopyProgress> progressSubject;
}

record CopyProgress(
    string Name,
    long TotalMaxBytes,
    long TotalBytes,
    long PreviousTotalBytes,
    long CurrentMaxBytes,
    long CurrentBytes
);