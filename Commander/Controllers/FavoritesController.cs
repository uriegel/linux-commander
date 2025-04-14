using Commander.UI;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

namespace Commander.Controllers;

// TODO 1. remove all times before filling new
// TODO 2. fill parent item
// TODO 3. append add favorite
class FavoritesController : ControllerBase<FavoritesItem>, IController, IDisposable
{
    #region IController

    public string CurrentPath { get; private set; } = "fav";

    public int Directories { get; private set; }

    public int Files { get; }

    public int HiddenDirectories => 0;

    public int HiddenFiles => 0;

    public bool CheckRestriction(string searchKey)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task CopyItems(string? targetPath, bool move) => Unit.Value.ToAsync();

    public Task CreateFolder() => Unit.Value.ToAsync();

    public Task DeleteItems()
    {
        // TODO
        throw new NotImplementedException();
    }

    public async Task<int> Fill(string path, FolderView folderView)
    {
        return -1; // !!! 0
    }

    public ExifData? GetExifData(int pos) => null;

    public string? GetItemPath(int pos)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task<string?> OnActivate(int pos)
    {
        // TODO
        throw new NotImplementedException();
    }

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
    {
        model.UnselectRange(pos, count);
        return 0;
    }

    public Task Rename()
    {
        // TODO
        throw new NotImplementedException();
    }

    public void SelectAll(FolderView folderView) { }
    public void SelectCurrent(FolderView folderView) { }
    public void SelectNone(FolderView folderView) { }
    public void SelectToEnd(FolderView folderView) { }
    public void SelectToStart(FolderView folderView) { }

    #endregion

    public FavoritesController(FolderView folderView) : base()
        => folderView.SetController(this);

    public override ColumnViewSubClassed.Column<FavoritesItem>[] GetColumns()
        => [
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
                Title = "Pfad",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End),
                OnLabelBind = i => i.Path
            }
        ];

    void OnIconNameBind(ListItemHandle listItem, FavoritesItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = "user-home";
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
    }
}

record FavoritesItem(string Name, string Path);
    