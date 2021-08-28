using System.Collections.Generic;
using GtkDotNet;

class CopyProcessor
{
    public void AddJobs(IEnumerable<string> sourcePaths, string destinationPath)
    {
        foreach (var sourcePath in sourcePaths)
            queue.AddJob(new ProcessingJob(ProcessingAction.Copy, sourcePath, destinationPath));
    }
    public CopyProcessor(ProgressControl progress)
        => queue.OnProgress += (s, args) => progress.Progress = args.Current / args.Total;
    readonly ProcessingQueue queue = new();
}