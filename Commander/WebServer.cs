using System.IO;
using UwebServer;
using UwebServer.Routes;

static class WebServer
{
    public static void Start() => server.Start();

    // TODO: Call stop
    public static void Stop() => server.Stop();

    static WebServer()
    {
        // TODO: from Resource
        var routeWebSite = new WebSite(file => File.OpenRead(Path.Combine("/home/uwe/Projekte/URiegel.WebServer/webroot/Reitbeteiligung", file)));

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