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

    public FolderContext Context { get; } = new();

    public FolderView(nint obj)
        : base(obj)
    {
        MultiSelection = true;
        OnSelectionChanged = SelectionChanged;
        controller = new(this);
        OnCreated();

        async void OnCreated()
        {
            await Task.Delay(1);
            pathEditing = Handle
                            .GetParent()
                            .DataContext(Context)
                            .GetFirstChild<EditableLabelHandle>();
            pathEditing.OnNotify("editing",
                    e =>
                    {
                        if ((bool)e.GetProperty("editing", typeof(bool))! == false)
                        {
                            columnView.GrabFocus();
                            if (!string.IsNullOrEmpty(Context.CurrentPath))
                                controller.ChangePath(Context.CurrentPath);
                        }
                    }).Binding("text", "CurrentPath", BindingFlags.Bidirectional);
            _ = Handle.AddController(EventControllerKey.New().OnKeyPressed((c, k, m) =>
            {
                if (k == 9)
                    StopRestriction();
                else if (k == 22)
                {
                    MainContext.Instance.Restriction = MainContext.Instance.Restriction?[..^1];
                    FilterChanged(FilterChange.LessStrict);
                }
                else
                {
                    var key = (char)gdk_keyval_to_unicode(c);
                    var searchKey = MainContext.Instance.Restriction + key;
                    if (controller.CheckRestriction(searchKey))
                    {
                        MainContext.Instance.Restriction = searchKey;
                        FilterChanged(FilterChange.MoreStrict);
                    }
                }
                return false;
            }));
        }
    }

    public static FolderView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as FolderView;

    public void StartPathEditing() => pathEditing.StartEditing();

    public void OnPathChanged() => StopRestriction();

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
            Context.SelectedPath = controller.GetItemPath(CurrentPos);
            Context.ExifData = controller.GetExifData(CurrentPos);
        }
    }

    public event EventHandler? OnFocusEnter;
    public event EventHandler? OnFocusLeave;

    public void GrabFocus() => columnView.GrabFocus();

    public void Refresh()
    {
        var pos = controller.GetFocusedItemPos();
        var model = columnView.GetModel<SelectionHandle>();
        var nil = new SelectionHandle();
        columnView.SetModel(nil);
        columnView.SetModel(model);
        ScrollTo(pos);
    }

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

    void FocusEnter() => OnFocusEnter?.Invoke(this, EventArgs.Empty);

    void FocusLeave()
    {
        StopRestriction();
        OnFocusLeave?.Invoke(this, EventArgs.Empty);
    }

    void StopRestriction()
    {
        MainContext.Instance.Restriction = null;
        FilterChanged(FilterChange.LessStrict);
    }

    void OnActivate(int pos) => controller.OnActivate(pos);

    readonly FolderController controller;
    EditableLabelHandle pathEditing = new(0);
    bool mouseButton;
    bool mouseButtonCtrl;
    
    [System.Runtime.InteropServices.DllImport("libgtk-4.so.1")]
    private static extern int gdk_keyval_to_unicode(int keyval);
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }