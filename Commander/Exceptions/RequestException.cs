using CsTools.HttpRequest;

class RequestException : Exception
{
    public CustomRequestError? CustomRequestError { get; }
    public RequestError RequestError { get; }

    public RequestException(RequestError e) : base(e.StatusText)
    {
        RequestError = e;
        CustomRequestError = e.GetCustomRequestError();
    }
    
}