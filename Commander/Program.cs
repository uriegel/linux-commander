using GtkDotNet;
using GtkDotNet.Controls;

const string appId = "de.uriegel.commander";
Application
    .NewAdwaita(appId)
        .OnActivate(app => app
            .SubClass(ManagedApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
            .ManagedApplicationWindow()
            .SaveBounds(appId, 600, 800)
            .Show())
        .Run(0, IntPtr.Zero);

// TODO Json defaults to CsTools
// TODO Settings to Gtk4DotNet
// TODO Bounds to Gtk4DotNet
// TODO ColumnView, ToggleButton: change columns and models

// TODO Preview pane
// TODO 2 panes with labels or buttons
// TODO Subclass FolderView
// TODO Subclass ColumnView
