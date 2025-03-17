using Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

// TODO size column like Commander
// TODO ellipse wrapping in cell
// TODO root Icons

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