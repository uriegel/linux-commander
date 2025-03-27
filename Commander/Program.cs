using GtkDotNet;
using GtkDotNet.Controls;

using Commander.Settings;
using Commander.UI;
using WebServerLight;
using Commander;

const string appId = "de.uriegel.commander";
{
    Gtk.ShowDiagnostics();

    var server =
        ServerBuilder
            .New()
            .Http(20000)
            .WebsiteFromResource()
            .Get(WebRequests.OnGet)
            .UseRange()
            .Build();
    server.Start();
    
    Gtk.ShowDiagnostics();

    using var app = Application
        .NewAdwaita(appId)
            .OnActivate(app =>
                app
                .SubClass(ManagedApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
                .SubClass(new FolderViewPanedClass(p => new FolderViewPaned(p)))
                .SubClass(new FolderViewClass())
                .ManagedApplicationWindow()
                .SaveBounds(appId, 600, 800)
                .Show());
    app.Run(0, IntPtr.Zero);
}
Gtk.ShowDiagnostics();


