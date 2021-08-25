using System;
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

    var themeAction = new GtkAction("themes", "themeAdwaita", SetTheme);
    app.AddActions(new[] {
        new GtkAction("destroy", window.Close, "<Ctrl>Q"), 
        new GtkAction("showhidden", false, showHidden, "<Ctrl>H"),
        themeAction,
        new GtkAction("devtools", webView.Inspector.Show, "F12"), 
        new GtkAction("viewer", false, showViewer, "F3"), 
    });    
    window.Add(webView);
    webView.LoadUri($"http://localhost:9865");
    webView.Settings.EnableDeveloperExtras = true;

    webView.ScriptDialog += (s, e) =>
    {
        if (e.Message.StartsWith("!!webmsg!!"))
        {
            var text = e.Message[10..];
            var pos = text.IndexOf("!!");
            var msg = text[..pos];
            var param = text[(pos+2)..];
            switch (msg)
            {
                case "theme":
                    themeAction.SetStringState(param);
                    break;
                case "title":
                    headerBar.SetSubtitle(param);
                    break;
                default:
                    Console.WriteLine($"{msg} {param}");
                    break;
            }
        }
    };

    var settings = new Settings("de.uriegel.commander");

    window.Delete += (s, e) => 
    {
        var (w, h) = (s as Window).Size;
        var (x, y) = (s as Window).GetPosition();
        settings.SetInt("window-width", w);
        settings.SetInt("window-height", h);
        settings.SetInt("window-position-x", x);
        settings.SetInt("window-position-y", y);
        settings.SetBool("is-maximized", window.IsMaximized());
        WebServer.Stop();
    };    
    
    app.AddWindow(window);

    var w = settings.GetInt("window-width");
    var h = settings.GetInt("window-height");
    var x = settings.GetInt("window-position-x");
    var y = settings.GetInt("window-position-y");
    window.SetDefaultSize(w, h);
    if (settings.GetBool("is-maximized"))
        window.Maximize();
    if (x != -1 && y != -1)
        window.Move(x, y);    
    window.ShowAll();

    void SetTheme(string theme) => webView.RunJavascript($"setTheme('{theme}')");
    void showHidden(bool show) => webView.RunJavascript($"showHidden({(show?"true":"false")})");
    void showViewer(bool show) => webView.RunJavascript($"showViewer({(show?"true":"false")})");
});

// TODO: Exif-Datetime: check request
// TODO: CreateFolder
// TODO: Delete 
// TODO: Delete with progress
// TODO: css-styles in GTK