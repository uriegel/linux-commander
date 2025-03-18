using Commander.UI;

namespace Commander.Controllers;

// TODO OnPathChanged when enter or double click
// TODO Check if Controller changed => new Controller
// TODO changePath in controller
// TODO DirectoryController

class FolderController
{
    public void Set(FolderView folderView) => controller.Set(folderView);

    public void Fill() => controller.Fill();

    public IController? OnActivate(uint pos)
    {
        var newPath = controller.OnActivate(pos);
        if (!string.IsNullOrEmpty(newPath))
        {
            // detect new controller
            Console.WriteLine($"Neuer Pfad: {newPath}");
        }
        return null;
    }

    IController controller = new Root();
}