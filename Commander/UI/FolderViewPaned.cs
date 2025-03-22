using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class FolderViewPaned(nint obj) : SubClassInst<PanedHandle>(obj)
{
    public static FolderViewPaned? GetInstance(PanedHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderViewPaned;

    public void OnDown(WindowHandle window) => folderViewActive?.OnDown(window);
    public void OnUp(WindowHandle window) => folderViewActive?.OnUp(window);
    public void OnPageDown(WindowHandle window) => folderViewActive?.OnPageDown(window);
    public void OnPageUp(WindowHandle window) => folderViewActive?.OnPageUp(window);
    public void OnHome() => folderViewActive?.OnHome();
    public void OnEnd() => folderViewActive?.OnEnd();
    
    public void SelectAll() => folderViewActive?.OnSelectAll();
    public void SelectNone() => folderViewActive?.OnSelectNone();

    protected override async void OnCreate()
    {
        Handle.AddController(EventControllerKey.New().OnKeyPressed((_, k, m)
            =>
        {
            if (k == 23)
            {
                GetInactiveFolderView()?.GrabFocus();
                return true;
            }
            else
                return false;
        }));

        await Task.Delay(1);

        var cvhl = Handle.GetStartChild<CustomColumnViewHandle>();
        var cvhr = Handle.GetEndChild<CustomColumnViewHandle>();
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


