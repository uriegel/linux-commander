using System;
using System.Threading.Tasks;
using GtkDotNet;
using UwebServer;
using UwebServer.Routes;

static class WebServer
{
    public static void Start() => server.Start();
    public static void Stop() => server.Stop();

    static WebServer()
    {
        var startTime = DateTime.Now;
        var routeWebSite = new WebSite(file => new ResourceStream($"/de/uriegel/commander/web/{file}"), _ => startTime);
        var routeService = new JsonService("/commander", input => 
        {
            switch (input.Path)
            {
                case "getitems":
                    var getItems = input.RequestParam.Get<GetItems>();
                    var items = DirectoryProcessor.GetItems(getItems.Path, getItems.HiddenIncluded);
                    return Task.FromResult<object>(items);
                default:
                    return Task.FromResult<object>(new { Name = "Uwe Riegel", EMail = "uriegel@web.de" });
            }
        });

        server = new(new()
        {
            Port = 9865,
            Routes = new Route[]
            {
                routeService,
                routeWebSite
            }
        });
    }

    static readonly Server server;
}

record GetItems(string Id, string Path, bool HiddenIncluded);