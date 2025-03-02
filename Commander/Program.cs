using GtkDotNet;

Application
    .NewAdwaita("de.uriegel.commander")
        .OnActivate(app =>
            app
                .NewWindow()
                    .Title("Commander")
                    .DefaultSize(600, 800)
                    .Show())
        .Run(0, IntPtr.Zero);

