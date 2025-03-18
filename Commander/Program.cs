using GtkDotNet;
using GtkDotNet.Controls;

using Commander.Settings;
using Commander.UI;

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


