using GtkDotNet;

const string appId = "de.uriegel.commander";
Application
    .NewAdwaita(appId)
        .OnActivate(app => app
            .SubClass(new MainWindow.MainWindowClass())
            .CustomWindow("MainWindow")
            .SaveBounds(appId, 600, 800)
            .Show())
        .Run(0, IntPtr.Zero);

// TODO Preview pane
// TODO 2 panes with labels or buttons
// TODO Subclass FolderView
// TODO Subclass ColumnView? or one subclass FolderView
// TODO Json defaults to CsTools
// TODO Settings to Gtk4DotNet
// TODO Bounds to Gtk4DotNet
