using System;

class ExceptionEventArgs : EventArgs
{
    public string Exception { get; }
    public string[] IDs { get;  }

    public ExceptionEventArgs(string exception, string[] ids)
    {
        Exception = exception;
        IDs = ids;
    }
}