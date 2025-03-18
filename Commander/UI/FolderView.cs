using Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;


// TODO Controller for Controllers
// TODO OnPathChanged when enter or double click
// TODO Check if Controller changed => new Controller
// TODO changePath in controller
// TODO DirectoryController

class FolderView(nint obj) : ColumnViewSubClassed(obj)
{
    public uint FindPos(nint item)
    {
        var model = columnView.GetModel<SelectionHandle>();
        var items = model.GetRawItems();
        return (uint)items.TakeWhile(n => n != item).Count();
    }

    protected override void OnCreate()
    {
        controller.Set(this);
        controller.Fill();
    }

    protected override void OnFinalize()
    {
        Console.WriteLine("ColumnView finalized");
    }
    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    readonly IController controller = new Root();
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }