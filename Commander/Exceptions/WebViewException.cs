using System;

class WebViewException : Exception
{
    public string[] IDs { get;  }
    public WebViewException(string text, string[] ids) : base(text)
        => IDs = ids;
}