using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GtkDotNet;

namespace Commander.DataContexts;

class CopyProgressContext : INotifyPropertyChanged
{
    public static CopyProgressContext Instance = new();

    public static object GetTotalFraction(object? copyProgress)
    {
        var cp = copyProgress as CopyProgress;
        return cp != null
            ? ((double)cp.TotalBytes + (double)cp.CurrentBytes) / (double)cp.TotalMaxBytes
            : 0;
    }

    public static object GetFraction(object? copyProgress)
    {
        var cp = copyProgress as CopyProgress;
        return cp != null
            ? cp.CurrentMaxBytes != 0
            ? (double)cp.CurrentBytes / (double)cp.CurrentMaxBytes
            : 0
            : 0;
    }

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

    public void Start(string title, long totalSize, int count)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        CopyProgress = new CopyProgress(
            title, 
            "",
            count,
            0, 
            totalSize,
            0,
            0,
            0,
            0,
            true
        );
    }

    public void SetNewFileProgress(string name, long size, int index)
    {
        var currentSize = Instance.CopyProgress?.PreviousTotalBytes ?? 0;
        Instance.CopyProgress = (Instance.CopyProgress ?? new("", "", 0, 0, 0, 0, 0, 0, 0, false)) with
        {
            Name = name,
            CurrentCount = index,
            TotalBytes = currentSize,
            CurrentMaxBytes = size,
            PreviousTotalBytes = currentSize + size,
            CurrentBytes = 0
        };
    }

    public async void Stop()
    {
        Instance.CopyProgress = (Instance.CopyProgress ?? new("", "", 0, 0, 0, 0, 0, 0, 0, false)) with
        {
            IsRunning = false
        };

        try
        {
            await Task.Delay(5000, cts?.Token ?? default);
            CopyProgress = null;
        }
        catch (TaskCanceledException) { }
    }

    public void SetProgress(long totalSize, long size)
    {
        var newVal = (Instance.CopyProgress ?? new("", "", 0, 0, 0, 0, 0, 0, 0, false)) with
        {
            CurrentBytes = size,
        };

        if (size < totalSize)
            progressSubject.OnNext(newVal);
        else
            Instance.CopyProgress = newVal;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    static CopyProgressContext()
    {
        progressSubject = new();
        progressSubject
            .Sample(TimeSpan.FromMilliseconds(80))
            .Subscribe(value =>
            {
                if (value.CurrentCount == Instance.CopyProgress?.CurrentCount)
                    Instance.CopyProgress = value;
            });
    }

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));

    CancellationTokenSource? cts;

    static readonly Subject<CopyProgress> progressSubject;
}

record CopyProgress(
    string Title,
    string Name,
    int TotalCount,
    int CurrentCount,
    long TotalMaxBytes,
    long TotalBytes,
    long PreviousTotalBytes,
    long CurrentMaxBytes,
    long CurrentBytes,
    bool IsRunning

);