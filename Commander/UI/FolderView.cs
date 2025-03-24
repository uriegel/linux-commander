using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using Commander.Controllers;
using System.ComponentModel;
using Commander.DataContexts;
using Commander.EventArg;

namespace Commander.UI;

class FolderView : ColumnViewSubClassed
{
    public int CurrentPos { get; private set; } = -1;

    public event EventHandler<PosChangedEventArgs>? PosChanged; 
    public event EventHandler<ItemsCountChangedEventArgs>? ItemsCountChanged;

    public FolderView(nint obj)
        : base(obj)
    {
        MultiSelection = true;
        OnSelectionChanged = SelectionChanged;
        controller = new(this);
        controller.ItemsCountChanged += (s, e) => ItemsCountChanged?.Invoke(this, e);
    }

    public static FolderView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderView;

    public void ScrollTo(int pos)
    {
        columnView.ScrollTo(pos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(pos);
    }

    public void OnDown()
    {
        var pos = controller.GetFocusedItemPos();
        var count = controller.ItemsCount();
        var newPos = Math.Min(pos + 1, count - 1);
        columnView.ScrollTo(newPos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(newPos);
    }

    public void OnUp()
    {
        var pos = controller.GetFocusedItemPos();
        var newPos = Math.Max(pos - 1, 0);
        columnView.ScrollTo(newPos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(newPos);
    }

    public void OnPageDown(WindowHandle window)
    {
        var pageSize = GetNumberOfVisibleRows(window);
        var pos = controller.GetFocusedItemPos();
        var count = controller.ItemsCount();
        var newPos = Math.Min(pos + pageSize, count - 1);
        columnView.ScrollTo(newPos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(newPos);
    }

    public void OnPageUp(WindowHandle window)
    {
        var pageSize = GetNumberOfVisibleRows(window);
        var pos = controller.GetFocusedItemPos();
        var newPos = Math.Max(pos - pageSize, 0);
        columnView.ScrollTo(newPos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(newPos);
    }

    public void OnHome()
    {
        columnView.ScrollTo(0, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(0);
    }

    public void OnEnd()
    {
        var newPos = controller.ItemsCount() - 1;
        columnView.ScrollTo(newPos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(newPos);
    }

    public void OnSelectAll() => controller.SelectAll();
    public void OnSelectNone() => controller.SelectNone();
    public void OnSelectCurrent() => controller.SelectCurrent();
    public void OnSelectToStart() => controller.SelectToStart();
    public void OnSelectToEnd() => controller.SelectToEnd();

    public void SelectCurrent()
    {
        var pos = controller.GetFocusedItemPos();
        var count = controller.ItemsCount();
        var model = columnView.GetModel<SelectionHandle>();
        var isSelected = model.IsSelected(pos);
        if (isSelected)
            model.UnselectItem(pos);
        else
            model.SelectItem(pos, false);
        columnView.ScrollTo(Math.Min(pos + 1, count - 1), ListScrollFlags.ScrollFocus);
    }

    public void SelectToStart()
    {
        var pos = controller.GetFocusedItemPos();
        var model = columnView.GetModel<SelectionHandle>();
        model.SelectRange(0, pos + 1, true);
    }

    public void SelectToEnd()
    {
        var pos = controller.GetFocusedItemPos();
        var count = controller.ItemsCount();
        var model = columnView.GetModel<SelectionHandle>();
        model.SelectRange(pos, count - pos, true);
    }

    public SelectionHandle GetSelectionModel() => columnView.GetModel<SelectionHandle>();

    void CheckCurrentChanged(int newPos)
    {
        if (newPos != CurrentPos)
        {
            CurrentPos = newPos;
            PosChanged?.Invoke(this, new(controller.GetItemPath(CurrentPos)));
        }
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

    void SelectionChanged(nint model, int pos, int count)
    {
        controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);
        CheckCurrentChanged(controller.GetFocusedItemPos());
    }

    void FocusEnter()
    {
        OnFocusEnter?.Invoke(this, EventArgs.Empty);
        PosChanged?.Invoke(this, new(controller.GetItemPath(CurrentPos)));
    } 

    void FocusLeave() => OnFocusLeave?.Invoke(this, EventArgs.Empty);

    void OnActivate(int pos) => controller.OnActivate(pos);

    readonly FolderController controller;
    bool mouseButton;
    bool mouseButtonCtrl;
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }