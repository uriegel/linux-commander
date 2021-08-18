using System;
using System.IO;
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

        server = new(new()
        {
            Port = 9865,
            Routes = new Route[]
            {
                routeWebSite
            }
        });
    }

    static readonly Server server;
}