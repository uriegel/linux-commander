using Commander.UI;
using GtkDotNet;
using GtkDotNet.SafeHandles;

using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

// TODO FolderType (parent/folder/item)
// TODO Icon for those types
// TODO sort parent -> folder by name -> item by sort
// TODO SortModel detect descending /ascending, but only item sort to outside

class DirectoryController : Controller<DirectoryItem>, IController
{
    #region IController

    public async void Fill(string path)
    {
        var result = await Task.Factory.StartNew(() => DirectoryProcessing.GetFiles(path));
        RemoveAll();
        Insert(result.Items);
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
            new()
            {
                Title = "Datum",
                Resizeable = true,
                OnItemSetup = () => Label.New(),
                OnLabelBind = i => i.Time.ToString()
            },
            new()
            {
                Title = "Größe",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size.ToString()
            }
        ];

    static void OnIconNameBind(ListItemHandle listItem, DirectoryItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        label?.Set(item.Name);
    }
}


