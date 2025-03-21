using Commander.UI;
using GtkDotNet.SafeHandles;

namespace Commander.Controllers;

class FolderController(FolderView folderView)
{

    // TODO
    public uint GetRowItemPos(WidgetHandle widget) => controller.GetRowItemPos(widget);

    public void OnActivate(uint pos)
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
                folderView.SelectItem((uint)lastPos, true);
        }

    }

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

