using GtkDotNet;
using GtkDotNet.Controls;
using WebServerLight;

using Commander.Settings;
using Commander.UI;
using WebServerLight.Routing;
using Commander;

var server =
    WebServer
        .New()
        .Http(20000)
        .WebsiteFromResource()
        .Route(MethodRoute
            .New(Method.Post)
            .Add(PathRoute
                .New("/request")
                .Request(Requests.Process)))
        .Route(MethodRoute
            .New(Method.Get)
            .Add(PathRoute
                .New("/iconfromname")
                .Request(Requests.GetIconFromName))
            .Add(PathRoute
                .New("/iconfromextension")
                .Request(Requests.GetIconFromExtension))
            .Add(PathRoute
                .New("/getfile")
                .Request(Requests.GetFile))
            .Add(PathRoute
                .New("/gettrack")
                .Request(Requests.GetTrack)))
        .AddAllowedOrigin("http://localhost:5173")
        .AccessControlMaxAge(TimeSpan.FromMinutes(5))
        .WebSocket(Requests.WebSocket)
        .UseRange()
        .Build();
server.Start();

using var app = Application
    .NewAdwaita(Globals.AppId)
        .OnActivate(app =>
            app
            .SubClass(ManagedAdwApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
            .SubClass(ProgressControl.Subclass())
            .SubClass(ProgressSpinner.Subclass())
            .ManagedAdwApplicationWindow()
            .SaveBounds(600, 800)
            .Show());
app.Run(0, IntPtr.Zero);

server.Stop();




