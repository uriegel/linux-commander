using GtkDotNet;

var app = new Application("de.uriegel.commander");
WebServer.Start();
app.Run(() =>
{
    app.RegisterResources();
    using var builder = Builder.FromResource("/de/uriegel/commander/main_window.glade");

    var window = new Window(builder.GetObject("window"));
    var headerBar = new HeaderBar(builder.GetObject("headerbar"));
    var webView = new WebView();
    window.Add(webView);
    webView.LoadUri($"http://localhost:9865");
    webView.Settings.EnableDeveloperExtras = true;

    var settings = new Settings("de.uriegel.commander");

    window.Configure += (s, e) => 
    {
        var (w, h) = (s as Window).Size;
        settings.SetInt("window-width", w);
        settings.SetInt("window-height", h);
        settings.SetBool("is-maximized", window.IsMaximized());
    };    
    
    app.AddWindow(window);

    var w = settings.GetInt("window-width");
    var h = settings.GetInt("window-height");
    window.SetDefaultSize(w, h);
    if (settings.GetBool("is-maximized"))
        window.Maximize();
    // window.Move(2900, 456);
    window.ShowAll();
});



