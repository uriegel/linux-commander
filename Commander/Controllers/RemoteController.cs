using Commander.Enums;
using Commander.Settings;
using Commander.UI;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using static CsTools.Extensions.Core;
using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

class RemoteController : ControllerBase<DirectoryItem>, IController
{
    #region IController

    public string CurrentPath { get; private set; } = "remote";

    public int Directories { get; private set; }

    public int Files { get; }

    public int HiddenDirectories => 0;

    public int HiddenFiles => 0;

    public bool CheckRestriction(string searchKey) => false;

    public Task CopyItems(string? targetPath, bool move) => Unit.Value.ToAsync();

    public Task CreateFolder() => Unit.Value.ToAsync();

    public async Task DeleteItems()
    {
    }

    public Task<int> Fill(string path, FolderView folderView)
    {
        return (-1).ToAsync();
    }

    public ExifData? GetExifData(int pos) => null;

    public string? GetItemPath(int pos) => null;

    public async Task<string?> OnActivate(int pos)
    {
        return null;
    }

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
    {
        model.UnselectRange(pos, count);
        return 0;
    }

    public async Task Rename()
    {
    }

    public void SelectAll(FolderView folderView) { }
    public void SelectCurrent(FolderView folderView) { }
    public void SelectNone(FolderView folderView) { }
    public void SelectToEnd(FolderView folderView) { }
    public void SelectToStart(FolderView folderView) { }

    #endregion

    public RemoteController(FolderView folderView) : base()
        => folderView.SetController(this);

    public override Column<DirectoryItem>[] GetColumns() =>
        [
            new()
            {
                Title = "Name",
                Expanded = true,
                Resizeable = true,
                OnSort = DirectoryController.OnNameSort,
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
                OnSort = DirectoryController.OnTimeSort,
                OnItemSetup = () => Label.New(),
                OnItemBind = OnTimeBind
            },
            new()
            {
                Title = "Größe",
                OnSort = DirectoryController.OnSizeSort,
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != -1 ? i.Size.ToString() : ""
            }
        ];

    static void OnIconNameBind(ListItemHandle listItem, DirectoryItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.Kind switch
        {
            ItemKind.Parent => "go-up",
            ItemKind.Folder => "folder-open",
            _ => DirectoryController.GetIconName(item.Name)
        };
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
        box?.GetParent<WidgetHandle>()?.GetParent().AddCssClass("hiddenItem", item.IsHidden);
    }

    static void OnTimeBind(ListItemHandle listItem, DirectoryItem item)
    {
        var label = listItem.GetChild<LabelHandle>();
        label?.Set(item.ExifData?.DateTime.ToString() ?? item.Time.ToString() ?? "");
        label?.AddCssClass("exif", item.ExifData?.DateTime != null);
    }

}

record RemoteItem(string Name, string IP, bool IsAndroid);
    