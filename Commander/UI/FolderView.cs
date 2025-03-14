using Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

class FolderViewClass()
        : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p))
    { }

class FolderView(nint obj) : ColumnViewSubClassed(obj)
{
    public uint FindPos(nint item)
    {
        var model = columnView.GetModel<SelectionHandle>();
        var items = model.GetItems<GObjectHandle>();
        return (uint)(items?.TakeWhile(n => n.GetInternalHandle() != item).Count() ?? -1);
    }

    protected override void OnCreate()
    {
        SetController(rootController);
        // controller.Fill();
    }

    protected override void OnFinalize()
    {
        Console.WriteLine("ColumnView finalized");
    }
    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    static readonly Root rootController = new();
}

