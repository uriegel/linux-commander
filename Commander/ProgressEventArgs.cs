using System;

class ProgressEventArgs : EventArgs
{
    public long Current { get;  }
    public long Total { get;  }
    public string[] IDs { get;  }
    public ProgressEventArgs(string[] ids, long total, long current)
    {
        IDs = ids;
        Current = current;
        Total = total;
    }
}