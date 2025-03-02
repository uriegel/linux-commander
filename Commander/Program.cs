using GtkDotNet;

const string appId = "de.uriegel.commander";
Application
    .NewAdwaita(appId)
        .OnActivate(app =>
            app
                .NewWindow()
                    .Title("Commander")
                    .SaveBounds(appId, 600, 800)
                    .Show())
        .Run(0, IntPtr.Zero);

// TODO Json defaults to CsTools
// TODO Settings to Gtk4DotNet
// TODO Bounds to Gtk4DotNet
