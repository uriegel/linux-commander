﻿using GtkDotNet;
using GtkDotNet.Controls;
using WebServerLight;

using Commander;
using Commander.Settings;
using Commander.UI;
using WebServerLight.Routing;

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
    Gtk.ShowDiagnostics();

    var server =
        WebServer
            .New()
            .Http(20000)
            .WebsiteFromResource()
            .Route(MethodRoute
                .New(Method.Get)
                .Request(WebRequests.OnGet))
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
                .SubClass(new AlertDialogClass())
                .SubClass(ProgressControl.Subclass())
                .SubClass(ProgressSpinner.Subclass())
                .ManagedAdwApplicationWindow()
                .SaveBounds(600, 800)
                .Show());
    app.Run(0, IntPtr.Zero);
}

Gtk.ShowDiagnostics();


