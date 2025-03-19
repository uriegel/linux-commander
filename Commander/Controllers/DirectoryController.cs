using Commander.Controllers;
using Commander.Enums;
using Commander.UI;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using static GtkDotNet.Controls.ColumnViewSubClassed;

class DirectoryController : Controller<DirectoryItem>, IController
{
    #region IController

    public void Fill()
    {

    }

    public string? OnActivate(uint pos)
    {
        return null;
    }

    #endregion

    public DirectoryController(FolderView folderView)
        => folderView.SetController(this);

    public override Column<DirectoryItem>[] GetColumns() =>
        [
            new()
            {
                Title = "Name",
                Expanded = true,
                Resizeable = true,
                OnItemSetup = ()
                    => Box
                        .New(Orientation.Horizontal)
                        .Append(Image.NewFromIconName("mail", IconSize.Menu).MarginStart(3).MarginEnd(3))
                        .Append(Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End)),
                OnItemBind = OnIconNameBind
            },
        ];

    static void OnIconNameBind(ListItemHandle listItem, DirectoryItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        label?.Set(item.Name);
    }
}

record DirectoryItem(string Name);
