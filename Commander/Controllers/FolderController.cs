using Commander.DataContexts;
using Commander.UI;

using static System.Console;

namespace Commander.Controllers;

class FolderController
{
    public FolderController(FolderView folderView)
    {
        this.folderView = folderView;
        controller = new RootController(folderView);
        Actions.Instance.PropertyChanged += (s, e) =>
        {
            folderView.Context.CurrentDirectories = Actions.Instance.ShowHidden ? controller.Directories + controller.HiddenDirectories : controller.Directories;
            folderView.Context.CurrentFiles = Actions.Instance.ShowHidden ? controller.Files + controller.Files : controller.Files;
        };
    }

    public string CurrentPath { get => controller.CurrentPath; }

    public string? GetItemPath(int pos) => controller.GetItemPath(pos);
    public ExifData? GetExifData(int pos) => controller.GetExifData(pos);

    public void DeleteItems() => controller.DeleteItems();

    public void OnActivate(int pos)
    {
        var newPath = controller.OnActivate(pos);
        if (!string.IsNullOrEmpty(newPath))
            ChangePath(newPath);
    }

    public async void ChangePath(string path)
    {
        DetectController(path);
        if (controller != null)
        {
            try
            {
                var lastPos = await controller.Fill(path, folderView);
                if (lastPos != -1)
                    folderView.ScrollTo(lastPos);
                folderView.Context.CurrentDirectories = Actions.Instance.ShowHidden ? controller.Directories + controller.HiddenDirectories : controller.Directories;
                folderView.Context.CurrentFiles = Actions.Instance.ShowHidden ? controller.Files + controller.Files : controller.Files;
                folderView.Context.CurrentPath = controller.CurrentPath;
                folderView.OnPathChanged();
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                    MainContext.Instance.ErrorText = "Kein Zugriff";
                else if (e is DirectoryNotFoundException)
                    MainContext.Instance.ErrorText = "Pfad nicht gefunden";    
                else
                    MainContext.Instance.ErrorText = "Ordner konnte nicht gewechselt werden";
                var refreshPath = folderView.Context.CurrentPath;
                folderView.Context.CurrentPath = "";
                folderView.Context.CurrentPath = refreshPath;
                ChangePath(folderView.Context.CurrentPath);
                Error.WriteLine($"Konnte Pfad nicht ändern: {e}");
            }
        }
    }

    public bool CheckRestriction(string searchKey) => controller.CheckRestriction(searchKey);

    public int GetFocusedItemPos() => controller.GetFocusedItemPos();
    public int ItemsCount() => controller.ItemsCount();

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
        => controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);

    public void SelectAll() => controller.SelectAll(folderView);
    public void SelectNone() => controller.SelectNone(folderView);
    public void SelectCurrent() => controller.SelectCurrent(folderView);
    public void SelectToStart() => controller.SelectToStart(folderView);
    public void SelectToEnd() => controller.SelectToEnd(folderView);

    bool DetectController(string path)
    {
        switch (path)
        {
            case "root":
            case "":
                if (controller is not RootController)
                {
                    controller.Dispose();
                    controller = new RootController(folderView);
                    return true;
                }
                break;
            default:
                if (controller is not DirectoryController)
                {
                    controller.Dispose();
                    controller = new DirectoryController(folderView);
                    return true;
                }
                break;
        }
        return false;
    }

    IController controller;
    FolderView folderView;
}

