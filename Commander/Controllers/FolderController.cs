using Commander.UI;
using GtkDotNet.SafeHandles;

namespace Commander.Controllers;

class FolderController(FolderView folderView)
{
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

    public int GetFocusedItemPos(WindowHandle window) => controller.GetFocusedItemPos();
    public int ItemsCount() => controller.ItemsCount();

    public void OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
        => controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);

    public void SelectAll() => controller.SelectAll(folderView);
    public void SelectNone() => controller.SelectNone(folderView);
    public void SelectCurrent(WindowHandle window) => controller.SelectCurrent(folderView, window);
    public void SelectToStart(WindowHandle window) => controller.SelectToStart(folderView, window);
    public void SelectToEnd(WindowHandle window) => controller.SelectToEnd(folderView, window);

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

