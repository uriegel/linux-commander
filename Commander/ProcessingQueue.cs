using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GtkDotNet;

class ProcessingQueue
{
    public event EventHandler<ProgressEventArgs> OnProgress;
    public event EventHandler<FinishEventArgs> Finish;

    public void AddJob(ProcessingJob processingJob)
    {
        lock (locker)
        {
            long length = 1;
            if (processingJob.Action == ProcessingAction.Delete)
                totalBytes += length;
            else if (File.Exists(processingJob.Source))
            {
                var fi = new FileInfo(processingJob.Source);
                length = fi.Length;
                totalBytes += length;
            }
            Job job = new(processingJob, length);
            if (!ids.Contains(processingJob.Id))
                ids = ids.Concat(new[] { processingJob.Id }).ToArray();
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
            Job job;
            lock (locker)
            {
                if (!jobs.TryDequeue(out job))
                {
                    Finish?.Invoke(this, new(ids));
                    proccessingThread = null;
                    alreadyProcessedBytes = 0;
                    totalBytes = 0;
                    ids = new string[0];
                    return;
                }
            }
            try
            {
                switch (job.ProcessingJob.Action)
                {
                    case ProcessingAction.Copy:
                        JobCopy(job);
                        break;
                    case ProcessingAction.Delete:
                        JobDelete(job.ProcessingJob);
                        break;
                }
            }
            catch (Exception e)
            {
                // TODO Capture exception
                alreadyProcessedBytes += job.FileSize;
            }
        }
    }

    void JobCopy(Job job)
    {
        GFile.Copy(job.ProcessingJob.Source, job.ProcessingJob.Destination, FileCopyFlags.None, Progress);
        alreadyProcessedBytes += job.FileSize;
    }

    void JobDelete(ProcessingJob job)
    {
        GFile.Trash(job.Source);
        Progress(1, 1);
        alreadyProcessedBytes += 1;
    }

    void Progress(long current, long total) 
        => OnProgress?.Invoke(this, new(totalBytes, current + alreadyProcessedBytes));
    
    readonly Queue<Job> jobs = new();

    string[] ids = new string[0];
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

record ProcessingJob(string Id, ProcessingAction Action, string Source, string Destination);
record Job(ProcessingJob ProcessingJob, long FileSize);

