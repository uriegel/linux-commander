using System.Drawing;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class MainWindow(nint obj) : ManagedAdwApplicationWindow(obj)
{
    public static AdwApplicationWindowHandle MainWindowHandle { get; private set; } = new();

    public static void Register(ApplicationHandle app)
        => app.SubClass(new MainWindowClass());

    protected override void OnCreate()
    {
        var webkitType = GType.Get(GTypeEnum.WebKitWebView);
        GType.Ensure(webkitType);

        MainWindowHandle = Handle;
        Handle.InitTemplate();

        webView = MainWindowHandle.GetTemplateChild<WebViewHandle, ApplicationWindowHandle>("webview");
        webView.DisableContextMenu();
        webView.BackgroundColor(Color.Transparent);
#if DEBUG
        webView.LoadUri("http://localhost:5173");
#else
        webView.LoadUri("http://localhost:20000");
#endif        
    }

    protected override void Initialize()
    {
        Handle.AddActions(
            [
                new("devtools", ShowDevTools, "<Ctrl><Shift>I"),
                new("refresh", () => Requests.SendMenuCommand("refresh"), "<Ctrl>R"),
                new("showhidden", false, show => Requests.SendMenuCheck("showhidden", show), "<Ctrl>H")
            ]);
    }

    public class MainWindowClass()
        : SubClass<AdwApplicationWindowHandle>(GTypeEnum.AdwApplicationWindow, "MainWindow", p => new MainWindow(p))
    { }

    protected override AdwApplicationWindowHandle CreateHandle(nint obj) => new(obj);

    void ShowDevTools()
    {
        var inspector = webView.GetInspector();
        inspector.Show();
        webView.GrabFocus();
        DetachInspector();

        async void DetachInspector()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(600));
            inspector.Detach();
        }
    }

    WebViewHandle webView = new();
}