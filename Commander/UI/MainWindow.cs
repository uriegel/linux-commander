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

        var webView = MainWindowHandle.GetTemplateChild<WebViewHandle, ApplicationWindowHandle>("viewer");
        webView.LoadUri("http://localhost:20000/index.html");
    }

    public class MainWindowClass()
        : SubClass<AdwApplicationWindowHandle>(GTypeEnum.AdwApplicationWindow, "MainWindow", p => new MainWindow(p))
    { }

    protected override AdwApplicationWindowHandle CreateHandle(nint obj) => new(obj);
}