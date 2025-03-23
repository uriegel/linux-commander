using Commander.UI;

namespace Commander.Controllers;

class FolderController(FolderView folderView)
{
    public string CurrentPath { get => controller.CurrentPath; }

    public string? GetItemPath(int pos) => controller.GetItemPath(pos);

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
            var lastPos = await controller.Fill(path);
            if (lastPos != -1)
                folderView.ScrollTo(lastPos);
        }
    }

    public int GetFocusedItemPos() => controller.GetFocusedItemPos();
    public int ItemsCount() => controller.ItemsCount();

    public void OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
        => controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);

    public void SelectAll() => controller.SelectAll(folderView);
    public void SelectNone() => controller.SelectNone(folderView);
    public void SelectCurrent() => controller.SelectCurrent(folderView);
    public void SelectToStart() => controller.SelectToStart(folderView);
    public void SelectToEnd() => controller.SelectToEnd(folderView);

    void DetectController(string path)
    {
        switch (path)
        {
            case "root":
                if (controller is not RootController)
                {
                    controller.Dispose();
                    controller = new RootController(folderView);
                }
                break;
            default:
                if (controller is not DirectoryController)
                {
                    controller.Dispose();
                    controller = new DirectoryController(folderView);
                }
                break;
        }
    }

    IController controller = new RootController(folderView);
}

