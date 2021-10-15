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
    public event EventHandler<ExceptionEventArgs> OnException;

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
            ids = ids.Concat(processingJob.ids).Distinct().ToArray();
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
        try
        {
            while (true)
            {
                Job job;
                lock (locker)
                {
                    if (!jobs.TryDequeue(out job))
                    {
                        Stop();
                        return;
                    }
                }
                switch (job.ProcessingJob.Action)
                {
                    case ProcessingAction.Copy:
                        JobCopy(job);
                        break;
                    case ProcessingAction.Move:
                        JobMove(job);
                        break;
                    case ProcessingAction.Delete:
                        JobDelete(job.ProcessingJob);
                        break;
                }
            }
        }
        catch (DeleteException)
        {
            OnException?.Invoke(this, new("Die Datei kann nicht gelöscht werden", ids));
            jobs.Clear();
            Stop();
        }
        catch (AccessDeniedException)
        {
            OnException?.Invoke(this, new("Zugriff verweigert", ids));
            jobs.Clear();
            Stop();
        }
        catch (TargetExistingException)
        {
            OnException?.Invoke(this, new("Die Zieldatei existiert bereits", ids));
            jobs.Clear();
            Stop();
        }
        catch (SourceNotFoundException)
        {
            OnException?.Invoke(this, new("Die zu kopierende Datei ist nicht vorhanden", ids));
            jobs.Clear();
            Stop();
        }
        catch (Exception)
        {
            OnException?.Invoke(this, new("Fehler aufgetreten", ids));
            jobs.Clear();
            Stop();
        }

        void Stop()
        {
            Finish?.Invoke(this, new(ids));
            proccessingThread = null;
            alreadyProcessedBytes = 0;
            totalBytes = 0;
            ids = new string[0];
        }
    }       

    void JobCopy(Job job)
    {
        GFile.Copy(job.ProcessingJob.Source, job.ProcessingJob.Destination, FileCopyFlags.Overwrite, true, Progress);
        alreadyProcessedBytes += job.FileSize;
    }

    void JobMove(Job job)
    {
        GFile.Move(job.ProcessingJob.Source, job.ProcessingJob.Destination, FileCopyFlags.Overwrite, true, Progress);
        alreadyProcessedBytes += job.FileSize;
    }

    void JobDelete(ProcessingJob job)
    {
        try
        {
            GFile.Trash(job.Source);
            Progress(1, 1);
            alreadyProcessedBytes += 1;
        }
        catch 
        {
            throw new DeleteException();
        }
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

record ProcessingJob(string[] ids, ProcessingAction Action, string Source, string Destination);
record Job(ProcessingJob ProcessingJob, long FileSize);

