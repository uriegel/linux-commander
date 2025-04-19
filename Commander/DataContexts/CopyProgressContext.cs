using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Commander.UI;

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

    public static object GetEstimatedDuration(object? copyProgress)
    {
        var cp = copyProgress as CopyProgress;
        return (cp != null && cp.Duration > ThreeSeconds)
            ? (cp.Duration / (double)GetTotalFraction(copyProgress)) - cp.Duration
            : TimeSpan.FromMilliseconds(0);
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

    public void SetRunning(bool running = true)
    {
        IsRunning = running;
        MainContext.Instance.InfoText = null;  
    } 

    public static bool CanClose()
    {
        if (!Instance.IsRunning)
            return true;
        else
        {
            return false;
        }
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

    public bool IsRunning { get; private set; }

    public CancellationToken Start(string title, long totalSize, int count)
    {
        cts?.Cancel();
        startTime = DateTime.Now;
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
            true,
            TimeSpan.FromMilliseconds(0)
        );
        return cts.Token;
    }

    public void SetNewFileProgress(string name, long size, int index)
    {
        var currentSize = Instance.CopyProgress?.PreviousTotalBytes ?? 0;
        if (Instance.CopyProgress != null)
            Instance.CopyProgress = Instance.CopyProgress with
            {
                Name = name,
                CurrentCount = index,
                TotalBytes = currentSize,
                CurrentMaxBytes = size,
                PreviousTotalBytes = currentSize + size,
                CurrentBytes = 0,
                Duration = DateTime.Now - startTime
            };
    }

    public async void Stop()
    {
        if (Instance.CopyProgress != null)
            Instance.CopyProgress = Instance.CopyProgress with
            {
                IsRunning = false,
                Duration = DateTime.Now - startTime
            };

        try
        {
            await Task.Delay(5000, cts?.Token ?? default);
            CopyProgress = null;
        }
        catch (TaskCanceledException) { }
    }

    public static void Cancel()
    {
        MainContext.Instance.InfoText = "Kopiervorgang wird abgebrochen...";
        Instance.cts?.Cancel();
        Instance.CopyProgress = null;
        FolderViewPaned.Instance.GetActiveFolderView()?.GrabFocus();
    }

    public void SetProgress(long totalSize, long size)
    {
        if (Instance.CopyProgress != null)
        {
            var newVal = Instance.CopyProgress with
            {
                CurrentBytes = size,
                Duration = DateTime.Now - startTime
            };

            if (size < totalSize)
                progressSubject.OnNext(newVal);
            else
                Instance.CopyProgress = newVal;
        }
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

    static readonly Subject<CopyProgress> progressSubject;

    readonly static TimeSpan ThreeSeconds = TimeSpan.FromSeconds(3);

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));

    CancellationTokenSource? cts;
    DateTime startTime = DateTime.Now;
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
    bool IsRunning,
    TimeSpan Duration
);