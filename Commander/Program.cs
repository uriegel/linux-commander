using GtkDotNet;
using GtkDotNet.Controls;

using Commander.Settings;
using Commander.UI;

const string appId = "de.uriegel.commander";
Application
    .NewAdwaita(appId)
        .OnActivate(async app =>
        {
            var win = app
                .SubClass(ManagedApplicationWindowClass.Register(p => new MainWindow(p), "mainwindow"))
                .SubClass(new FolderViewPanedClass(p => new FolderViewPaned(p)))
                .SubClass(new FolderViewClass())
                .ManagedApplicationWindow()
                .SaveBounds(appId, 600, 800);
            await Task.Delay(50);
            win.Show();
        })
        .Run(0, IntPtr.Zero);


