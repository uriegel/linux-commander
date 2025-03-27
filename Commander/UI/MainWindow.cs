using Commander.DataContexts;
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
        var viewer = Handle.GetTemplateChild<WidgetHandle, ApplicationWindowHandle>("viewer");
        Handle.OnRealize(w =>
            {
                var width = w.GetWidth();
                panedHandle?.SetPosition(width / 2);
            });
        Handle.OnSizeChanged((w, _) => panedHandle?.SetPosition(w / 2));
        Handle.DataContext(MainContext.Instance);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("statusText")
            ?.Binding("label", "SelectedPath", BindingFlags.Default)
            ?.Binding("visible", "Restriction", BindingFlags.Default, s => s == null);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("restrictionText")
            ?.Binding("label", "Restriction", BindingFlags.Default)
            ?.Binding("visible", "Restriction", BindingFlags.Default, s => s != null);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("labelDirs")
            ?.Binding("label", "CurrentDirectories", BindingFlags.Default);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("labelFiles")
            ?.Binding("label", "CurrentFiles", BindingFlags.Default);
        Handle.AddActions(
            [
                // new("togglePreviewMode", Events.MenuAction.Apply("TOGGLE_PREVIEW"), "<Ctrl>F3"),
                new("showpreview", false, show =>
                    {
                        viewer?.SetVisible(show);
                        if (initial)
                        {
                            // TODO Maximize window 
                            initial = false;
                            var viewerPaned = Handle.GetTemplateChild<PanedHandle, ApplicationWindowHandle>("viewerPaned");
                            viewerPaned?.SetPosition(Handle.GetHeight() / 2);
                        }
                    }, "F3"),
                // new("copy", Events.MenuAction.Apply("COPY"), "F5"),
                // new("createfolder", Events.MenuAction.Apply("CREATE_FOLDER"), "F7"),
                // new("adaptpath", Events.MenuAction.Apply("ADAPT_PATH"), "F9"),
                // new("delete", Events.MenuAction.Apply("DELETE")),
                // new("refresh", Events.MenuAction.Apply("REFRESH"), "<Ctrl>R"),
                new("showhidden", false, show => Actions.Instance.ShowHidden = show, "<Ctrl>H"),
                new("devtools", Gtk.ShowDiagnostics, "<Ctrl><Shift>I"),
                new("quit", Handle.CloseWindow, "<Ctrl>Q"),
                new("down", () => paned?.OnDown(), "Down"),
                new("up", () => paned?.OnUp(), "Up"),
                new("pageDown", () => paned?.OnPageDown(Handle), "Page_Down"),
                new("pageUp", () => paned?.OnPageUp(Handle), "Page_Up"),
                new("home", () => paned?.OnHome(), "Home"),
                new("end", () => paned?.OnEnd(), "End"),
                new("selectall", () => paned?.SelectAll(), "KP_Add"),
                new("selectnone", () => paned?.SelectNone(), "KP_Subtract"),
                new("selectcurrent", () => paned?.SelectCurrent(), "Insert"),
                new("selectToStart", () => paned?.SelectToStart(), "<Shift>Home"),
                new("selectToEnd", () => paned?.SelectToEnd(), "<Shift>End"),
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

    bool initial = true;
}

