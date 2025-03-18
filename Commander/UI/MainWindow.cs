using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class MainWindow(nint obj) : ManagedApplicationWindow(obj)
{
    public static void Register(ApplicationHandle app)
        => app.SubClass(new MainWindowClass());

    protected override void OnCreate()
    {
        Handle.InitTemplate();
        StyleContext.AddProviderForDisplay(
            Display.GetDefault(),
            CssProvider.New().FromResource("style"), StyleProviderPriority.Application);
    }

    protected override void Initialize()
    {
        var paned = Handle.GetTemplateChild<PanedHandle, ApplicationWindowHandle>("paned");
        Handle.OnRealize(w =>
            {
                var width = w.GetWidth();
                paned?.SetPosition(width / 2);
            });
        paned?.AddController(EventControllerKey.New().OnKeyPressed((_, k, m)
            => {
                if (k == 23)
                {
                    Paned.OnTab(paned, m);
                    return true;
                }
                else
                    return false;
            }));
        Handle.OnSizeChanged((w, _) => paned?.SetPosition(w / 2));

        // TODO Add to descriptions in Gtk4DotNet:
        // TODO <Ctrl>F3  b e f o r e  <F3> !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Handle.AddActions(
            [
                // new("togglePreviewMode", Events.MenuAction.Apply("TOGGLE_PREVIEW"), "<Ctrl>F3"),
                // new("showpreview", false, Events.PreviewAction, "F3"),
                // new("copy", Events.MenuAction.Apply("COPY"), "F5"),
                // new("createfolder", Events.MenuAction.Apply("CREATE_FOLDER"), "F7"),
                // new("adaptpath", Events.MenuAction.Apply("ADAPT_PATH"), "F9"),
                // new("delete", Events.MenuAction.Apply("DELETE")),
                // new("refresh", Events.MenuAction.Apply("REFRESH"), "<Ctrl>R"),
                // new("showhidden", false, Events.ShowHiddenAction, "<Ctrl>H"),
                // new("devtools", webView.ShowDevTools, "<Ctrl><Shift>I"),
                new("quit", Handle.CloseWindow, "<Ctrl>Q"),
            ]);
    }

    public class MainWindowClass()
        : SubClass<ApplicationWindowHandle>(GTypeEnum.ApplicationWindow, "MainWindow", p => new MainWindow(p))
    {
        protected override void ClassInit(nint cls, nint _)
        {
            // var webkitType = GType.Get(GTypeEnum.WebKitWebView);
            // GType.Ensure(webkitType);
            // var type = "WebKitWebView".TypeFromName();
            base.ClassInit(cls, _);
            InitTemplateFromResource(cls, "mainwindow");
        }
    }

    protected override void OnFinalize() => Console.WriteLine("Window finalized");
    protected override ApplicationWindowHandle CreateHandle(nint obj) => new(obj);
}

