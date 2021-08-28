using GtkDotNet;

class CopyProcessor
{
    public CopyProcessor(ProgressControl progress)
        => queue.OnProgress += (s, args) => progress.Progress = args.Current / args.Total;

    readonly ProcessingQueue queue = new();
}