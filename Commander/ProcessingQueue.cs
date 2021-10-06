using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GtkDotNet;

class ProcessingQueue
{
    public event EventHandler<ProgressEventArgs> OnProgress;

    public void AddJob(ProcessingJob job)
    {
        lock (locker)
        {
            if (File.Exists(job.Source))
            {
                var fi = new FileInfo(job.Source);
                totalBytes += fi.Length;
            }
            jobs.Enqueue(job);
            if (proccessingThread == null)
            {
                proccessingThread = new Thread(Process);
                proccessingThread.Start();
            }
        }
    }

    void Process()
    {
        while (true)
        {
            ProcessingJob job;
            lock (locker)
            {
                if (!jobs.TryDequeue(out job))
                {
                    proccessingThread = null;
                    // TODO Signal end
                    alreadyProcessedBytes = 0;
                    totalBytes = 0;
                    return;
                }
            }
            try
            {
                switch (job.Action)
                {
                    case ProcessingAction.Copy:
                        JobCopy(job);
                        break;
                    case ProcessingAction.Delete:
                        JobDelete(job);
                        break;
                }
            }
            catch (Exception e)
            {
                // TODO Capture exception
                if (File.Exists(job.Source))
                {
                    var fi = new FileInfo(job.Source);
                    // TODO: wrong file length, call Progress
                    OnProgress?.Invoke(this, new(totalBytes, fi.Length));
                }
            }
        }
    }

    void JobCopy(ProcessingJob job) => GFile.Copy(job.Source, job.Destination, FileCopyFlags.None, Progress);
    void JobDelete(ProcessingJob job) => GFile.Trash(job.Source);

    void Progress(long current, long total)
    {
        OnProgress?.Invoke(this, new(totalBytes, current + alreadyProcessedBytes));
        if (total == current)
            alreadyProcessedBytes += total;
    }

    readonly Queue<ProcessingJob> jobs = new();
    readonly object locker = new();
    long totalBytes;

    long alreadyProcessedBytes = 0;
    Thread proccessingThread;
}

enum ProcessingAction
{
    Copy, 
    Move,
    Delete
}

record ProcessingJob(ProcessingAction Action, string Source, string Destination);

