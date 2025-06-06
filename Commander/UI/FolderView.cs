using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using Commander.Controllers;
using System.ComponentModel;
using Commander.DataContexts;
using GtkDotNet.Exceptions;

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
        OnSortChanging = SortChanging;
        controller = new(this);

        Handle.AddController(EventControllerKey.New().OnKeyPressed((chr, modifiers) =>
        {
            if (chr == (char)ConsoleKey.Escape)
                StopRestriction();
            else if (chr == (char)ConsoleKey.Backspace)
            {
                if (MainContext.Instance.Restriction == null)
                {
                    var path = history.Get(modifiers.HasFlag(KeyModifiers.Shift));
                    if (path != null)
                        ChangePath(path, false);
                }
                else
                {
                    MainContext.Instance.Restriction = MainContext.Instance.Restriction?[..^1];
                    if (MainContext.Instance.Restriction?.Length == 0)
                        MainContext.Instance.Restriction = null;
                    FilterChanged(FilterChange.LessStrict);
                }
            }
            else
            {
                if (chr != '\0')
                {
                    var searchKey = MainContext.Instance.Restriction + chr;
                    if (controller.CheckRestriction(searchKey))
                    {
                        MainContext.Instance.Restriction = searchKey;
                        FilterChanged(FilterChange.MoreStrict);
                    }
                }
            }
            return false;
        }));
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        pathEditing = Handle
                        .GetParent()
                        .DataContext(Context)
                        .GetFirstChild<EditableLabelHandle>();
        pathEditing.OnNotify("editing",
            e =>
            {
                Context.IsEditing = (bool)e.GetProperty("editing", typeof(bool))! == true;
                if (!Context.IsEditing)
                {
                    columnView.GrabFocus();
                    var val = pathEditing.GetText();
                    if (val != null)
                        controller.ChangePath(val, true);
                }
            }).Binding("text", nameof(FolderContext.CurrentPath), BindingFlags.Default);
    }

    public static FolderView GetInstance(CustomColumnViewHandle handle)
        => (GetInstance(handle.GetInternalHandle()) as FolderView)!;

    public void StartPathEditing() => pathEditing.StartEditing();

    public void OnPathChanged(string? pathToSave)
    {
        StopRestriction();
        if (pathToSave != null)
            history.Set(pathToSave);
    } 

    public async void DeleteItems()
    {
        try
        {
            if (await controller.DeleteItems())
                Refresh();
        }
        catch (OperationCanceledException)
        {
            if (ProgressContext.Instance.CopyProgress != null)
                MainContext.Instance.ErrorText = "Löschvorgang ist unterbrochen worden...";
            Refresh();                
        }
        catch (GFileException e)
        {
            MainContext.Instance.ErrorText = e.Message;
        }
    }

    public void ShowFavorites() => ChangePath("fav", true);

    public async void Rename()
    {
        try
        {
            if (await controller.Rename())
                Refresh();
        }
        catch (IOException e) when (e.HResult == 13)
        {
            MainContext.Instance.ErrorText = "Kein Zugriff";
        }
        catch (IOException e2) when ((uint)e2.HResult == 0x80131620)
        {
            MainContext.Instance.ErrorText = "Ziel und Quelle sind identisch";
        }
    }

    public async void ExtendedRename()
    {
        if (await controller.ExtendedRename())
            Refresh();
    }

    public async void CreateFolder()
    {
        try
        {
            if (await controller.CreateFolder())
                Refresh();
        }
        catch (UnauthorizedAccessException)
        {
            MainContext.Instance.ErrorText = "Kein Zugriff";
        }
        catch (IOException e2) when ((uint)e2.HResult == 0x80131620)
        {
            MainContext.Instance.ErrorText = "Ziel und Quelle sind identisch";
        }
    }

    public async Task<bool> CopyItems(string? targetPath, bool move)
    {
        try
        {
            return await controller.CopyItems(targetPath, move);
        }
        catch (OperationCanceledException)
        {
            if (ProgressContext.Instance.CopyProgress != null)
                MainContext.Instance.ErrorText = "Kopiervorgang ist unterbrochen worden...";
            return true;
        }
        catch (GFileException gfe)
        {
            MainContext.Instance.ErrorText = gfe.Message;
            return false;
        }
        catch (RequestException re)
        {
            MainContext.Instance.ErrorText = re.Message;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

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

    public void OnPageDown()
    {
        var pageSize = GetNumberOfVisibleRows(MainWindow.MainWindowHandle);
        var pos = controller.GetFocusedItemPos();
        var count = controller.ItemsCount();
        var newPos = Math.Min(pos + pageSize, count - 1);
        columnView.ScrollTo(newPos, ListScrollFlags.ScrollFocus);
        CheckCurrentChanged(newPos);
    }

    public void OnPageUp()
    {
        var pageSize = GetNumberOfVisibleRows(MainWindow.MainWindowHandle);
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

    public void ChangePath(string path, bool saveHistory) => controller.ChangePath(path, saveHistory);

    public void GrabFocus() => columnView.GrabFocus();

    public void Refresh() => controller.ChangePath(controller.CurrentPath, false);

    public void InvalidateFocus()
        => Gtk.BeginInvoke(200, () =>
            {
                var pos = controller.GetFocusedItemPos();
                if (pos == -1)
                    pos = 0;
                // var model = columnView.GetModel<SelectionHandle>();
                // var nil = new SelectionHandle();
                // columnView.SetModel(nil);
                // columnView.SetModel(model);
                ScrollTo(pos);
            });

    protected override void OnCreate()
    {
        base.OnCreate();
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
        Context.SelectedFiles = controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);
        CheckCurrentChanged(controller.GetFocusedItemPos());
    }

    void SortChanging() => controller.OnSortChanging();

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
    readonly History history = new();
    EditableLabelHandle pathEditing = new(0);
    bool mouseButton;
    bool mouseButtonCtrl;
}

class FolderViewClass() : ColumnViewSubClassedClass("ColumnView", p => new FolderView(p)) { }