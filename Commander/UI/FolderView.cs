using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using Commander.Controllers;

namespace Commander.UI;

class FolderView : ColumnViewSubClassed
{
    public FolderView(nint obj) 
        : base(obj)
        => controller = new(this);

    public static FolderView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderView;

    public uint FindPos(nint item)
    {
        var model = columnView.GetModel<SelectionHandle>();
        var items = model.GetRawItems();
        return (uint)items.TakeWhile(n => n != item).Count();
    }

    public event EventHandler? OnFocus;

    public void GrabFocus() => columnView.GrabFocus();

    protected override void OnCreate()
    {
        OnActivate(OnActivate);
        controller.Fill();
        Handle.AddController(EventControllerFocus.New().OnEnter(OnFocusEnter));
    }

    protected override void OnFinalize()
        => Console.WriteLine("ColumnView finalized");

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    void OnFocusEnter() => OnFocus?.Invoke(this, new());

    void OnActivate(uint pos) => controller.OnActivate(pos);

    readonly FolderController controller;
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }