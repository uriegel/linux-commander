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
        var panedHandle = Handle.GetTemplateChild<PanedHandle, ApplicationWindowHandle>("paned");
        var paned = panedHandle != null ? FolderViewPaned.GetInstance(panedHandle) : null;
        Handle.OnRealize(w =>
            {
                var width = w.GetWidth();
                panedHandle?.SetPosition(width / 2);
            });
        Handle.OnSizeChanged((w, _) => panedHandle?.SetPosition(w / 2));
        Handle.AddActions(
            [
                // new("togglePreviewMode", Events.MenuAction.Apply("TOGGLE_PREVIEW"), "<Ctrl>F3"),
                // new("showpreview", false, Events.PreviewAction, "F3"),
                // new("copy", Events.MenuAction.Apply("COPY"), "F5"),
                // new("createfolder", Events.MenuAction.Apply("CREATE_FOLDER"), "F7"),
                // new("adaptpath", Events.MenuAction.Apply("ADAPT_PATH"), "F9"),
                // new("delete", Events.MenuAction.Apply("DELETE")),
                // new("refresh", Events.MenuAction.Apply("REFRESH"), "<Ctrl>R"),
                new("showhidden", false, show => Actions.Instance.ShowHidden = show, "<Ctrl>H"),
                new("devtools", Diagnose, "<Ctrl><Shift>I"),
                new("quit", Handle.CloseWindow, "<Ctrl>Q"),
                new("down", () => paned?.OnDown(Handle), "Down"),
                new("up", () => paned?.OnUp(Handle), "Up"),
                new("pageDown", () => paned?.OnPageDown(Handle), "Page_Down"),
                new("pageUp", () => paned?.OnPageUp(Handle), "Page_Up"),
                new("home", () => paned?.OnHome(), "Home"),
                new("end", () => paned?.OnEnd(), "End"),
                new("selectall", () => paned?.SelectAll(), "KP_Add"),
                new("selectnone", () => paned?.SelectNone(), "KP_Subtract"),
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

    void Diagnose()
    {
        GC.Collect();
        GC.Collect();
        Console.WriteLine($"Total memory: {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64:N0}, managed: {GC.GetTotalMemory(true):N0}");
    }
}

