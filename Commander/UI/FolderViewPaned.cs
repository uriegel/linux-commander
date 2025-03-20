using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class FolderViewPaned(nint obj) : SubClassInst<PanedHandle>(obj)
{
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
            folderViewActive?.GrabFocus();
            if (folderViewLeft != null)
                folderViewLeft.OnFocus += (s, e) => folderViewActive = folderViewLeft;
            folderViewRight = FolderView.GetInstance(cvhr);
            if (folderViewRight != null)
                folderViewRight.OnFocus += (s, e) => folderViewActive = folderViewRight;
            await Task.Delay(100);
            folderViewActive?.GrabFocus();
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


