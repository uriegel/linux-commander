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

    public void OnDown(WindowHandle window)
    {
        // TODO g_type_name
        var widget = window.GetFocus<WidgetHandle>();
        if (!widget.IsInvalid && widget.GetName() == "GtkColumnViewRowWidget")
            columnView.ScrollTo(8, ListScrollFlags.ScrollFocus);

        // {
        //     var next = widget.GetNextSibling<WidgetHandle>();
        //     if (!next.IsInvalid && next.GetName() == "GtkColumnViewRowWidget")
        //     {
        //         var sibling = widget.GetNextSibling<WidgetHandle>();
        //         if (!sibling.IsInvalid)
        //             sibling.GrabFocus();
        //     }
        // }
    }

    public event EventHandler? OnFocusEnter;
    public event EventHandler? OnFocusLeave;

    public void GrabFocus() => columnView.GrabFocus();

    protected override void OnCreate()
    {
        MultiSelection = true;
        Actions.Instance.PropertyChanged += OnActionChanged;
        OnActivate(OnActivate);
        Handle.AddController(EventControllerFocus
                                .New()
                                .OnEnter(FocusEnter)
                                .OnLeave(FocusLeave));
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

    void FocusEnter() => OnFocusEnter?.Invoke(this, new());
    void FocusLeave() => OnFocusLeave?.Invoke(this, new());

    void OnActivate(uint pos) => controller.OnActivate(pos);

    readonly FolderController controller;
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }