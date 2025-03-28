using Commander.UI;

namespace Commander.Controllers;

class FolderController(FolderView folderView)
{
    public string CurrentPath { get => controller.CurrentPath; }

    public string? GetItemPath(int pos) => controller.GetItemPath(pos);
    public ExifData? GetExifData(int pos) => controller.GetExifData(pos);

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
            var lastPos = await controller.Fill(path, folderView);
            if (lastPos != -1)
                folderView.ScrollTo(lastPos);

            folderView.Context.CurrentDirectories = controller.Directories;
            folderView.Context.CurrentFiles = controller.Files;
            folderView.Context.CurrentPath = controller.CurrentPath;
            folderView.OnPathChanged();
        }
    }

    public bool CheckRestriction(string searchKey) => controller.CheckRestriction(searchKey);

    public int GetFocusedItemPos() => controller.GetFocusedItemPos();
    public int ItemsCount() => controller.ItemsCount();

    public void OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
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

    IController controller = new RootController(folderView);
}

