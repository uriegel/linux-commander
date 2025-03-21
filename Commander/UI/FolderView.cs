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
    {
        MultiSelection = true;
        controller = new(this);
    }
    
    public static FolderView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderView;

    public void ScrollTo(uint pos) => columnView.ScrollTo(pos, ListScrollFlags.ScrollFocus);

    public void OnDown(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        columnView.ScrollTo((uint)pos + 1, ListScrollFlags.ScrollFocus);
    }

    public void OnUp(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        columnView.ScrollTo((uint)Math.Max(pos - 1, 0), ListScrollFlags.ScrollFocus);
    }

    public void OnHome() => columnView.ScrollTo(0, ListScrollFlags.ScrollFocus);

    public void OnEnd() => columnView.ScrollTo((uint)(controller.ItemsCount() - 1), ListScrollFlags.ScrollFocus);

    public event EventHandler? OnFocusEnter;
    public event EventHandler? OnFocusLeave;

    public void GrabFocus() => columnView.GrabFocus();

    protected override void OnCreate()
    {
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