using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using Commander.Controllers;
using System.ComponentModel;

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
        Actions.Instance.PropertyChanged += OnActionChanged;
        OnActivate(OnActivate);
        Handle.AddController(EventControllerFocus.New().OnEnter(OnFocusEnter));
        controller.ChangePath("root");
    }

    protected override void OnFinalize()
        => Console.WriteLine("ColumnView finalized");

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    void OnActionChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "ShowHidden":
                FilterChanged(Actions.Instance.ShowHidden ? FilterChange.LessStrict : FilterChange.MoreStrict);
                break;
        }
    }

    void OnFocusEnter() => OnFocus?.Invoke(this, new());

    void OnActivate(uint pos) => controller.OnActivate(pos);

    readonly FolderController controller;
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }