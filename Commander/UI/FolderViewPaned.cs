using System.Net.Http.Headers;
using System.Threading.Tasks;
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
                Console.WriteLine("Habe den Täb auch hier direkt");
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
            folderViewRight = FolderView.GetInstance(cvhr);
        }
    }

    protected override PanedHandle CreateHandle(nint obj) => new(obj);
    protected override void OnFinalize() => Console.WriteLine("FolderViewPaned finalized");

    FolderView? folderViewLeft;
    FolderView? folderViewRight;
}

class FolderViewPanedClass(Func<nint, FolderViewPaned> constructor)
    : SubClass<PanedHandle>(GTypeEnum.Paned, "FolderViewPaned", constructor) { }


