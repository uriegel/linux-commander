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
        OnSelectionChanged = SelectionChanged;
        controller = new(this);
    }

    public static FolderView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderView;

    public void ScrollTo(uint pos) => columnView.ScrollTo(pos, ListScrollFlags.ScrollFocus);

    public void OnDown(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        var count = controller.ItemsCount();
        columnView.ScrollTo((uint)Math.Min(pos + 1, count - 1), ListScrollFlags.ScrollFocus);
    }

    public void OnUp(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        columnView.ScrollTo((uint)Math.Max(pos - 1, 0), ListScrollFlags.ScrollFocus);
    }

    public void OnPageDown(WindowHandle window)
    {
        var pageSize = GetNumberOfVisibleRows(window);
        var pos = controller.GetFocusedItemPos(window);
        var count = controller.ItemsCount();
        columnView.ScrollTo((uint)Math.Min(pos + pageSize, count - 1), ListScrollFlags.ScrollFocus);
    }

    public void OnPageUp(WindowHandle window)
    {
        var pageSize = GetNumberOfVisibleRows(window);
        var pos = controller.GetFocusedItemPos(window);
        columnView.ScrollTo((uint)Math.Max(pos - pageSize, 0), ListScrollFlags.ScrollFocus);
    }

    public void OnHome() => columnView.ScrollTo(0, ListScrollFlags.ScrollFocus);

    public void OnEnd() => columnView.ScrollTo((uint)(controller.ItemsCount() - 1), ListScrollFlags.ScrollFocus);

    public void OnSelectAll() => controller.SelectAll();
    public void OnSelectNone() => controller.SelectNone();
    public void OnSelectCurrent(WindowHandle window) => controller.SelectCurrent(window);
    public void OnSelectToStart(WindowHandle window) => controller.SelectToStart(window);
    public void OnSelectToEnd(WindowHandle window) => controller.SelectToEnd(window);

    // TODO to Gtk4
    public void SetSelection(uint start, int count)
    {
        var model = columnView.GetModel<SelectionHandle>();
        if (start == 0 && count == -1)
            model.SelectAll();
    }

    // TODO to Gtk4
    public void UnselectAll()
    {
        var model = columnView.GetModel<SelectionHandle>();
        model.UnselectAll();
    }

    public void SelectCurrent(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        var count = controller.ItemsCount();
        var model = columnView.GetModel<SelectionHandle>();
        var isSelected = model.IsSelected((uint)pos);
        if (isSelected)
            model.UnselectItem((uint)pos);
        else
            model.SelectItem((uint)pos, false);
        columnView.ScrollTo((uint)Math.Min(pos + 1, count - 1), ListScrollFlags.ScrollFocus);
    }

    public void SelectToStart(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        var model = columnView.GetModel<SelectionHandle>();
        model.SelectRange(0, (uint)(pos + 1), true);
    }

    public void SelectToEnd(WindowHandle window)
    {
        var pos = controller.GetFocusedItemPos(window);
        var count = controller.ItemsCount();
        var model = columnView.GetModel<SelectionHandle>();
        model.SelectRange((uint)pos, (uint)(count - pos), true);
    }

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
        columnView?.AddController(GestureClick.New().OnPressed((i, d, b) =>
        {
            var status = columnView.GetDisplay().GetDefaultSeat().GetKeyboard().GetModifierState();
            mouseButton = true;
            mouseButtonCtrl = status.HasFlag(KeyModifiers.Control);
        })).AddController(GestureClick.New().OnReleased((i, d, b) =>
        {
            mouseButton = false;
            mouseButtonCtrl = false;
        }));
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

    int GetNumberOfVisibleRows(WindowHandle window)
    {
        var row = window.GetFocus<WidgetHandle>();
        if (!row.IsInvalid && row.GetName() == "GtkColumnViewRowWidget")
        {
            var parent = row.GetParent();
            var columnViewHeight = parent.GetHeight();
            var rowHeight = row.GetHeight();
            var countOfRows = columnViewHeight / (rowHeight + 2);
            return countOfRows;
        }
        else
            return 0;
    }

    void SelectionChanged(nint model, uint pos, uint count) => controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);

    void FocusEnter() => OnFocusEnter?.Invoke(this, EventArgs.Empty);
    void FocusLeave() => OnFocusLeave?.Invoke(this, EventArgs.Empty);

    void OnActivate(uint pos) => controller.OnActivate(pos);

    readonly FolderController controller;
    bool mouseButton;
    bool mouseButtonCtrl;
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }