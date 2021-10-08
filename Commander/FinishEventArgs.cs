using System;

class FinishEventArgs : EventArgs
{
    public string[] IDs { get;  }
    public FinishEventArgs(string[] ids) =>  IDs = ids;
}