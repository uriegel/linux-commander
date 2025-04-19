using GtkDotNet;
using GtkDotNet.Controls;
using WebServerLight;

using Commander;
using Commander.Settings;
using Commander.UI;

// TODO Viewer show pictures and media from remote
// TODO Padding in columns
// TODO Size column with dots
// TODO 0 Size in root

// TODO extended rename

// TODO Pdf viewer: PdViewer in WebWindowNetCore
// TODO Pdf viewer: PdViewer in Gtk4DotNet
// TODO Pdf viewer
// TODO Text viewer/editor
// TODO Track viewer some inconsistencies like max velocity too high, trackpoints not containing data any more...

// TODO When item is opened wait till process stops and refresh item

// TODO Rename remote
// TODO remote copy from and to

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


