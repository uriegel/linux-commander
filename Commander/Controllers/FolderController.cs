using Commander.UI;

namespace Commander.Controllers;

class FolderController(FolderView folderView)
{
    public void OnActivate(uint pos)
    {
        var newPath = controller.OnActivate(pos);
        if (!string.IsNullOrEmpty(newPath))
            ChangePath(newPath);
    }

    public void ChangePath(string path)
    {
        DetectController(path);
        controller?.Fill(path);
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

