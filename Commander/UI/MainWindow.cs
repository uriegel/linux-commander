using System.Drawing;
using CsTools.Extensions;
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
        Handle.DataContext(MainContext.Instance);
        Handle.GetTemplateChild<BannerHandle, ApplicationWindowHandle>("banner")
            .Binding("revealed", nameof(MainContext.ErrorText), BindingFlags.Default, v => v != null)
            .Binding("title", nameof(MainContext.ErrorText), BindingFlags.Default)
            .SideEffect(b => b.OnButtonClicked(() => b.SetRevealed(false)));
        Handle.AddActions(
            [
                new("devtools", ShowDevTools, "<Ctrl><Shift>I"),
                new("adaptpath", () => Requests.SendMenuCommand("adaptpath"), "F9"),
                new("refresh", () => Requests.SendMenuCommand("refresh"), "<Ctrl>R"),
                new("showhidden", false, show => Requests.SendMenuCheck("showhidden", show), "<Ctrl>H"),
                new("toggleViewMode", () => Requests.SendMenuCommand("togglepreview"), "<Ctrl>F3"),
                new("showpreview", false, show => Requests.SendMenuCheck("showpreview", show), "F3"),
                new("selectcurrent", () => Requests.SendMenuCommand("insert"), "Insert"),
                new("selectall", () => Requests.SendMenuCommand("selectall"), "KP_Add"),
                new("selectnone", () => Requests.SendMenuCommand("selectnone"), "KP_Subtract"),
                new("copy", () => Requests.SendMenuCommand("copy"), "F5"),
                new("move", () => Requests.SendMenuCommand("move"), "F6"),
                new("delete", () => Requests.SendMenuCommand("delete")),
                new("createfolder", () => Requests.SendMenuCommand("createfolder"), "F7"),
                new("renameascopy", () => Requests.SendMenuCommand("renameascopy"), "<Shift>F2"),
                new("extendedrename", () => Requests.SendMenuCommand("extendedrename"), "<Ctrl>F2"),
                new("rename", () => Requests.SendMenuCommand("rename"), "F2"),
                new("openfolder", () => Requests.SendMenuCommand("openfolder"), "<Ctrl>Return")
            ]);
        Handle.OnClose(OnClose);
    }

    public class MainWindowClass()
        : SubClass<AdwApplicationWindowHandle>(GTypeEnum.AdwApplicationWindow, "MainWindow", p => new MainWindow(p))
    { }

    protected override AdwApplicationWindowHandle CreateHandle(nint obj) => new(obj);

    bool OnClose(WindowHandle _)
    {
        if (!ProgressContext.CanClose())
        {
            var wh = Handle.GetTemplateChild<RevealerHandle, ApplicationWindowHandle>("progress-revealer");
            var progressControl = ProgressControl.GetInstance(wh);
            progressControl.ShowPopover();
            return true;
        }
        else
            return false;
    }

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