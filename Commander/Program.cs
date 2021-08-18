using GtkDotNet;

var app = new Application("de.uriegel.commander");
app.Run(() =>
{
    app.RegisterResources();
    using var builder = Builder.FromResource("/de/uriegel/commander/main_window.glade");

    var window = new Window(builder.GetObject("window"));
    var headerBar = new HeaderBar(builder.GetObject("headerbar"));

    app.AddWindow(window);

    window.ShowAll();
});



