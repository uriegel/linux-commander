using System;

class ProgressEventArgs : EventArgs
{
    public long Current { get;  }
    public long Total { get;  }
    public ProgressEventArgs(long total, long current)
    {
        Current = current;
        Total = total;
    }
}