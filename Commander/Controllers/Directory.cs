using Commander.Controllers;
using Commander.UI;
using static GtkDotNet.Controls.ColumnViewSubClassed;

class Directory : Controller<DirectoryItem>, IController
{
    #region IController

    public void Fill()
    {
        throw new NotImplementedException();
    }

    public string? OnActivate(uint pos)
    {
        throw new NotImplementedException();
    }

    #endregion

    public Directory(FolderView folderView)
        => folderView.SetController(this);

    public override Column<DirectoryItem>[] GetColumns() => [];
}

record DirectoryItem;
