using System;

class RefreshEventArgs : EventArgs
{
    public string Id { get;  }
    public RefreshEventArgs(string id) => Id = id;
}