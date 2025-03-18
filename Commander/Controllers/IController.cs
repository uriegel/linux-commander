using Commander.UI;

namespace Commander.Controllers;
interface IController
{
    void Set(FolderView folderView);

    void Fill();

    string? OnActivate(uint pos);
}
