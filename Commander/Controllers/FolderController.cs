using Commander.UI;

namespace Commander.Controllers;

// TODO OnPathChanged when enter or double click
// TODO Check if Controller changed => new Controller
// TODO changePath in controller
// TODO DirectoryController

class FolderController(FolderView folderView)
{
    public void Fill() => controller.Fill();

    public void OnActivate(uint pos)
    {
        var newPath = controller.OnActivate(pos);
        if (!string.IsNullOrEmpty(newPath))
        {
            DetectController(newPath);
        }
    }

    void DetectController(string path)
    {
        switch (path)
        {
            case "root":
                if (controller is not RootController)
                    controller = new RootController(folderView);
                break;
            default:
                if (controller is not DirectoryController)
                    controller = new DirectoryController(folderView);
                break;

        }
    }

    IController controller = new RootController(folderView);
}

