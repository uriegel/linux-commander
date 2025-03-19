using System.Threading.Tasks;
using Commander.UI;
using GtkDotNet;
using GtkDotNet.SafeHandles;

using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

class DirectoryController : Controller<DirectoryItem>, IController
{
    #region IController

    public async void Fill(string path)
    {
        var result = await Task.Factory.StartNew(() => DirectoryProcessing.GetFiles(path));
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
                OnLabelBind = i => i.Date.ToString()
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

record DirectoryItem(string Name, DateTime Date, long Size);
