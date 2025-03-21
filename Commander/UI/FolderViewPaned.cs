using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class FolderViewPaned(nint obj) : SubClassInst<PanedHandle>(obj)
{
    public static FolderViewPaned? GetInstance(PanedHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderViewPaned;

    public void OnDown(WindowHandle window) => folderViewActive?.OnDown(window);

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
                    IActionMap.GetAction("down").SetEnabled(true);
                };
                folderViewLeft.OnFocusLeave += (s, e) => IActionMap.GetAction("down").SetEnabled(false);
            }
            folderViewRight = FolderView.GetInstance(cvhr);
            if (folderViewRight != null)
            {
                folderViewRight.OnFocusEnter += (s, e) =>
                {
                    folderViewActive = folderViewRight;
                    IActionMap.GetAction("down").SetEnabled(true);
                };
                folderViewRight.OnFocusLeave += (s, e) => IActionMap.GetAction("down").SetEnabled(false);
            }
            await Task.Delay(100);
            folderViewActive?.GrabFocus();
            IActionMap.GetAction("down").SetEnabled(true);
        }
    }

    protected override PanedHandle CreateHandle(nint obj) => new(obj);
    protected override void OnFinalize() => Console.WriteLine("FolderViewPaned finalized");

    FolderView? GetInactiveFolderView() => folderViewActive == folderViewLeft ? folderViewRight : folderViewLeft;

    FolderView? folderViewLeft;
    FolderView? folderViewRight;
    FolderView? folderViewActive;
}

class FolderViewPanedClass(Func<nint, FolderViewPaned> constructor)
    : SubClass<PanedHandle>(GTypeEnum.Paned, "FolderViewPaned", constructor) { }


