using Commander.DataContexts;
using Commander.Settings;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class FolderViewPaned(nint obj) : SubClassWidgetInst<PanedHandle>(obj)
{
    public static FolderViewPaned GetInstance(PanedHandle handle)
        => (GetInstance(handle.GetInternalHandle()) as FolderViewPaned)!;

    public void OnDown() => folderViewActive?.OnDown();
    public void OnUp() => folderViewActive?.OnUp();
    public void OnPageDown() => folderViewActive?.OnPageDown();
    public void OnPageUp() => folderViewActive?.OnPageUp();
    public void OnHome() => folderViewActive?.OnHome();
    public void OnEnd() => folderViewActive?.OnEnd();
    
    public void SelectAll() => folderViewActive?.OnSelectAll();
    public void SelectNone() => folderViewActive?.OnSelectNone();
    public void SelectCurrent() => folderViewActive?.OnSelectCurrent();
    public void SelectToStart() => folderViewActive?.OnSelectToStart();
    public void SelectToEnd() => folderViewActive?.OnSelectToEnd();

    public void Refresh() => folderViewActive?.Refresh();

    public void AdaptPath()
    {
        var path = folderViewActive?.Context.CurrentPath;
        if (!string.IsNullOrEmpty(path))
            GetInactiveFolderView()?.ChangePath(path);
    }

    public void DeleteItems() => folderViewActive?.DeleteItems();
    public void Rename() => folderViewActive?.Rename();
    public void CreateFolder() => folderViewActive?.CreateFolder();
    public async void CopyItems(bool move)
    {
        if (CopyProgressContext.Instance.IsRunning)
        {
            MainContext.Instance.ErrorText = "Es ist bereits eine Aktion am Laufen";
            return;
        }
        var inactive = GetInactiveFolderView();
        try
        {
            CopyProgressContext.Instance.SetRunning();
            if (folderViewActive != null && await folderViewActive.CopyItems(GetInactiveFolderView()?.Context.CurrentPath, move))
            {
                inactive?.Refresh();
                if (move)
                    folderViewActive.Refresh();
            }
        }
        finally
        {
            CopyProgressContext.Instance.SetRunning(false);
        }
    } 
        
    protected override async void OnInitialize()
    {
        Handle.AddController(EventControllerKey.New().OnKeyPressed((_, k, m) =>
        {
            if (k == 23)
            {
                if (!m.HasFlag(KeyModifiers.Shift))
                    GetInactiveFolderView()?.GrabFocus();
                else
                    folderViewActive?.StartPathEditing();
                return true;
            }
            else
                return false;
        }));

        var window = Handle.GetAncestor<AdwApplicationWindowHandle>();
        var cvhl = window.GetTemplateChild<CustomColumnViewHandle, AdwApplicationWindowHandle>("columnview-left");
        var cvhr = window.GetTemplateChild<CustomColumnViewHandle, AdwApplicationWindowHandle>("columnview-right");
        folderViewLeft = FolderView.GetInstance(cvhl);
        folderViewActive = folderViewLeft;
        MainContext.Instance.ChangeFolderContext(folderViewLeft.Context);
        folderViewLeft.Context.IsLeft = true;
        folderViewLeft.ChangePath(Storage.Retrieve().LeftPath);
        folderViewLeft.OnFocusEnter += (s, e) =>
        {
            folderViewActive = folderViewLeft;
            MainContext.Instance.ChangeFolderContext(folderViewActive.Context);
            EnableKeyNavigation(true);
            if (folderViewActive?.Context.IsEditing != true)
                EnableFolderViewActions(true);
        };
        folderViewLeft.OnFocusLeave += (s, e) =>
        {
            EnableKeyNavigation(false);
            EnableFolderViewActions(false);
        };
        folderViewRight = FolderView.GetInstance(cvhr);
        folderViewRight.ChangePath(Storage.Retrieve().RightPath);
        folderViewRight.OnFocusEnter += (s, e) =>
        {
            folderViewActive = folderViewRight;
            MainContext.Instance.ChangeFolderContext(folderViewActive?.Context);
            EnableKeyNavigation(true);
            if (folderViewActive?.Context.IsEditing != true)
                EnableFolderViewActions(true);
        };
        folderViewRight.OnFocusLeave += (s, e) =>
        {
            EnableKeyNavigation(false);
            EnableFolderViewActions(false);
        };

        await Task.Delay(100);
        folderViewActive?.GrabFocus();
        IActionMap.GetAction("down").SetEnabled(true);
    }

    protected override PanedHandle CreateHandle(nint obj) => new(obj);
    protected override void OnFinalize() => Console.WriteLine("FolderViewPaned finalized");

    void EnableKeyNavigation(bool enable)
    {
        IActionMap.GetAction("down").SetEnabled(enable);
        IActionMap.GetAction("up").SetEnabled(enable);
        IActionMap.GetAction("pageDown").SetEnabled(enable);
        IActionMap.GetAction("pageUp").SetEnabled(enable);
        IActionMap.GetAction("home").SetEnabled(enable);
        IActionMap.GetAction("end").SetEnabled(enable);
    }

    void EnableFolderViewActions(bool enable)
    {
        Console.WriteLine("EnableFolderViewActions: " + enable);
        IActionMap.GetAction("delete").SetEnabled(enable);
    }

    FolderView? GetInactiveFolderView() => folderViewActive == folderViewLeft ? folderViewRight : folderViewLeft;

    FolderView? folderViewLeft;
    FolderView? folderViewRight;
    FolderView? folderViewActive;
}

class FolderViewPanedClass(Func<nint, FolderViewPaned> constructor)
    : SubClass<PanedHandle>(GTypeEnum.Paned, "FolderViewPaned", constructor) { }


