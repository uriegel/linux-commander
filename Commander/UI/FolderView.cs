using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using Commander.Controllers;

namespace Commander.UI;

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
        OnActivate(OnActivate);
        controller.Set(this);
        controller.Fill();
    }

    protected override void OnFinalize()
    {
        Console.WriteLine("ColumnView finalized");
    }

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    void OnActivate(uint pos) => controller.OnActivate(pos);

    readonly FolderController controller = new();
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }