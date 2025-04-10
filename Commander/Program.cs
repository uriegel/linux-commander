using GtkDotNet;
using GtkDotNet.Controls;
using WebServerLight;

using Commander;
using Commander.Settings;
using Commander.UI;
using GtkDotNet.SubClassing;

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
        .NewAdwaita(Globals.AppId)
            .OnActivate(app =>
                app
                .SubClass(ManagedAdwApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
                .SubClass(new FolderViewPanedClass(p => new FolderViewPaned(p)))
                .SubClass(new FolderViewClass())
                .SubClass(new ConflictViewClass())
                .SubClass(new ConflictDialogClass())
                .SubClass(new Commander.UI.AlertDialogClass())
                .SubClass(ProgressControl.Subclass())
                .SubClass(ProgressSpinner.Subclass())
                .ManagedAdwApplicationWindow()
                .SaveBounds(600, 800)
                .Show());
    app.Run(0, IntPtr.Zero);
}

Gtk.ShowDiagnostics();


