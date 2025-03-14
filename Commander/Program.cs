using GtkDotNet;
using GtkDotNet.Controls;

using Settings;

const string appId = "de.uriegel.commander";
Application
    .NewAdwaita(appId)
        .OnActivate(app => app
            .SubClass(ManagedApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
            .SubClass(new FolderViewClass())
            .ManagedApplicationWindow()
            .SaveBounds(appId, 600, 800)
            .Show())
        .Run(0, IntPtr.Zero);

// TODO CsTools 7.23.2 in GtkDotNet
// TODO Paned position controlling like webview: resize with relative position (30% or 50 % ...)
// TODO Json defaults to CsTools
// TODO protected void Initialize() MainWindow

