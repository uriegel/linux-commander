using Commander.DataContexts;
using Commander.Enums;
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
        StyleContext.AddProviderForDisplay(
            Display.GetDefault(),
            CssProvider.New().FromResource("style"), StyleProviderPriority.Application);
    }

    protected override void Initialize()
    {
        var panedHandle = Handle.GetTemplateChild<PanedHandle, ApplicationWindowHandle>("paned");
        var paned = FolderViewPaned.GetInstance(panedHandle);
        var viewer = Handle.GetTemplateChild<WidgetHandle, ApplicationWindowHandle>("viewer");
        Handle.OnRealize(w =>
            {
                var width = w.GetWidth();
                panedHandle.SetPosition(width / 2);
            });
        Handle.OnSizeChanged((w, _) => panedHandle.SetPosition(w / 2));
        Handle.OnClose(OnClose);
        Handle.DataContext(MainContext.Instance);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("statusText")
            .Binding("label", nameof(MainContext.SelectedPath), BindingFlags.Default)
            .Binding("visible", nameof(MainContext.StatusChoice), BindingFlags.Default, s => (StatusChoice?)s == StatusChoice.Status);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("selectedItemsText")
            .Binding("label", nameof(MainContext.SelectedFiles), BindingFlags.Default, s => (int?)s == 1 ? "1 Eintrag selektiert" : $"{s} Einträge selektiert")
            .Binding("visible", nameof(MainContext.StatusChoice), BindingFlags.Default, s => (StatusChoice?)s == StatusChoice.SelectedItems);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("restrictionText")
            .Binding("label", nameof(MainContext.Restriction), BindingFlags.Default, s => $"Einschränkung auf: {s}")
            .Binding("visible", nameof(MainContext.StatusChoice), BindingFlags.Default, s => (StatusChoice?)s == StatusChoice.Restriction);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("backgroundActionText")
            .Binding("label", nameof(MainContext.BackgroundAction), BindingFlags.Default, GetBackgroundAction)
            .Binding("visible", nameof(MainContext.StatusChoice), BindingFlags.Default, s => (StatusChoice?)s == StatusChoice.BackgroundAction);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("labelDirs")
            .Binding("label", nameof(MainContext.CurrentDirectories), BindingFlags.Default);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("labelFiles")
            .Binding("label", nameof(MainContext.CurrentFiles), BindingFlags.Default);
        Handle.GetTemplateChild<LabelHandle, ApplicationWindowHandle>("actionBar")
            .BindingToCss("info", nameof(MainContext.StatusChoice), s => (StatusChoice?)s == StatusChoice.BackgroundAction);
        Handle.GetTemplateChild<BannerHandle, ApplicationWindowHandle>("banner")
            .Binding("revealed", nameof(MainContext.ErrorText), BindingFlags.Default, v => v != null)
            .Binding("title", nameof(MainContext.ErrorText), BindingFlags.Default)
            .SideEffect(b => b.OnButtonClicked(() => b.SetRevealed(false)));
        Handle.GetTemplateChild<BannerHandle, ApplicationWindowHandle>("info-banner")
            .Binding("revealed", nameof(MainContext.InfoText), BindingFlags.Default, v => v != null)
            .Binding("title", nameof(MainContext.InfoText), BindingFlags.Default);

        Handle.AddActions(
            [
                new("favorites", paned.ShowFavorites, "F1"),
                new("extendedrename", paned.ExtendedRename, "<Shift>F2"),
                new("rename", paned.Rename, "F2"),
                new("toggleViewMode", Viewer.ToggleView, "<Ctrl>F3"),
                new("showpreview", false, show => Viewer.Show(Handle, viewer, show), "F3"),
                new("copy", () => paned.CopyItems(false), "F5"),
                new("move", () => paned.CopyItems(true), "F6"),
                new("createfolder", paned.CreateFolder, "F7"),
                new("adaptpath", paned.AdaptPath, "F9"),
                new("delete", paned.DeleteItems, "Delete"),
                new("refresh", paned.Refresh, "<Ctrl>R"),
                new("showhidden", false, show => Actions.Instance.ShowHidden = show, "<Ctrl>H"),
                new("diagnostics", Gtk.ShowDiagnostics, "<Ctrl><Shift>I"),
                new("quit", Handle.CloseWindow, "<Ctrl>Q"),
                new("down", paned.OnDown, "Down"),
                new("up", paned.OnUp, "Up"),
                new("pageDown", paned.OnPageDown, "Page_Down"),
                new("pageUp", paned.OnPageUp, "Page_Up"),
                new("home", paned.OnHome, "Home"),
                new("end", paned.OnEnd, "End"),
                new("selectall", paned.SelectAll, "KP_Add"),
                new("selectnone", paned.SelectNone, "KP_Subtract"),
                new("selectcurrent", paned.SelectCurrent, "Insert"),
                new("selectToStart", paned.SelectToStart, "<Shift>Home"),
                new("selectToEnd", paned.SelectToEnd, "<Shift>End"),
            ]);
    }

    public class MainWindowClass()
        : SubClass<AdwApplicationWindowHandle>(GTypeEnum.AdwApplicationWindow, "MainWindow", p => new MainWindow(p)) { }

    protected override void OnFinalize() => Console.WriteLine("Window finalized");
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

    string GetBackgroundAction(object? value)
    {
        if (value is BackgroundAction ba)
        {
            if (ba == BackgroundAction.ExifDatas)
                return "Exif-Daten werden geholt...";
        }
        return "";
    }
}