using GtkDotNet;
using GtkDotNet.Controls;
using WebServerLight;

using Commander.Settings;
using Commander.UI;
using WebServerLight.Routing;
using Commander;

// TODO Commander scaffold with GTK4 theme

// TODO pathEdit: when path is too long, it is not ellipsized: Custom Control containing entry and label

// TODO Pdf viewer: PdViewer in WebWindowNetCore
// TODO Pdf viewer: PdViewer in Gtk4DotNet
// TODO Pdf viewer
// TODO Text viewer/editor
// TODO Track viewer some inconsistencies like max velocity too high, trackpoints not containing data any more...

// TODO Drag n drop

// TODO When item is opened wait till process stops and refresh item

// TODO Rename remote

// TODO Android range

// TODO Dont copy directory from remote
{
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
            .AddAllowedOrigin("http://localhost:5173")                    
            .AccessControlMaxAge(TimeSpan.FromMinutes(5))
            .UseRange()
            .Build();
    server.Start();

    using var app = Application
        .NewAdwaita(Globals.AppId)
            .OnActivate(app =>
                app
                .SubClass(ManagedAdwApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
                .ManagedAdwApplicationWindow()
                .SaveBounds(600, 800)
                .Show());
    app.Run(0, IntPtr.Zero);
}

Gtk.ShowDiagnostics();


