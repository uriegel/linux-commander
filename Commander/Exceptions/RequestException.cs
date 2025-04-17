using CsTools.HttpRequest;

class RequestException(RequestError e) : Exception(e.StatusText)
{
    public RequestError RequestError { get; } = e;
}