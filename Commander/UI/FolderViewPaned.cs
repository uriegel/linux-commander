using Commander.DataContexts;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class FolderViewPaned(nint obj) : SubClassInst<PanedHandle>(obj)
{
    public static FolderViewPaned? GetInstance(PanedHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderViewPaned;

    public void OnDown() => folderViewActive?.OnDown();
    public void OnUp() => folderViewActive?.OnUp();
    public void OnPageDown(WindowHandle window) => folderViewActive?.OnPageDown(window);
    public void OnPageUp(WindowHandle window) => folderViewActive?.OnPageUp(window);
    public void OnHome() => folderViewActive?.OnHome();
    public void OnEnd() => folderViewActive?.OnEnd();
    
    public void SelectAll() => folderViewActive?.OnSelectAll();
    public void SelectNone() => folderViewActive?.OnSelectNone();
    public void SelectCurrent() => folderViewActive?.OnSelectCurrent();
    public void SelectToStart() => folderViewActive?.OnSelectToStart();
    public void SelectToEnd() => folderViewActive?.OnSelectToEnd();

    protected override async void OnCreate()
    {
        Handle.AddController(EventControllerKey.New().OnKeyPressed((_, k, m)
            =>
        {
            if (k == 23 && !m.HasFlag(KeyModifiers.Shift))
            {
                GetInactiveFolderView()?.GrabFocus();
                return true;
            }
            else
                return false;
        }));

        await Task.Delay(1);

        var window1 = Handle.GetAncestor<WindowHandle>();

        // TODO Gtk4!
        ApplicationWindowHandle window = new(window1.GetInternalHandle());

        var cvhl = window.GetTemplateChild<CustomColumnViewHandle, ApplicationWindowHandle>("columnview-left");
        var cvhr = window.GetTemplateChild<CustomColumnViewHandle, ApplicationWindowHandle>("columnview-right");
        if (cvhl != null && cvhr != null)
        {
            folderViewLeft = FolderView.GetInstance(cvhl);
            folderViewActive = folderViewLeft;
            if (folderViewLeft != null)
            {
                folderViewLeft.OnFocusEnter += (s, e) =>
                {
                    folderViewActive = folderViewLeft;
                    EnableKeyNavigation(true);
                };
                folderViewLeft.OnFocusLeave += (s, e) => EnableKeyNavigation(false);
                folderViewLeft.PosChanged += (s, e) => MainContext.Instance.CurrentPath = e.CurrentPath;
                folderViewLeft.ItemsCountChanged += (s, e) =>
                {
                    MainContext.Instance.CurrentDirectories = e.Directories.ToString();
                    MainContext.Instance.CurrentFiles = e.Items.ToString();
                };
                 
            }
            folderViewRight = FolderView.GetInstance(cvhr);
            if (folderViewRight != null)
            {
                folderViewRight.OnFocusEnter += (s, e) =>
                {
                    folderViewActive = folderViewRight;
                    EnableKeyNavigation(true);
                };
                folderViewRight.OnFocusLeave += (s, e) => EnableKeyNavigation(false);
                folderViewRight.PosChanged += (s, e) => MainContext.Instance.CurrentPath = e.CurrentPath;
                folderViewRight.ItemsCountChanged += (s, e) =>
                {
                    MainContext.Instance.CurrentDirectories = e.Directories.ToString();
                    MainContext.Instance.CurrentFiles = e.Items.ToString();
                };
            }
            await Task.Delay(100);
            folderViewActive?.GrabFocus();
            IActionMap.GetAction("down").SetEnabled(true);
        }
    }

    protected override PanedHandle CreateHandle(nint obj) => new(obj);
    protected override void OnFinalize() => Console.WriteLine("FolderViewPaned finalized");

    void EnableKeyNavigation(bool enable)
    {
        IActionMap.GetAction("down").SetEnabled(enable);
        IActionMap.GetAction("up").SetEnabled(enable);
        IActionMap.GetAction("pageDown").SetEnabled(enable);
        IActionMap.GetAction("pageUp").SetEnabled(enable);
        IActionMap.GetAction("home").SetEnabled(enable);
        IActionMap.GetAction("end").SetEnabled(enable);
    }

    FolderView? GetInactiveFolderView() => folderViewActive == folderViewLeft ? folderViewRight : folderViewLeft;

    FolderView? folderViewLeft;
    FolderView? folderViewRight;
    FolderView? folderViewActive;
}

class FolderViewPanedClass(Func<nint, FolderViewPaned> constructor)
    : SubClass<PanedHandle>(GTypeEnum.Paned, "FolderViewPaned", constructor) { }


