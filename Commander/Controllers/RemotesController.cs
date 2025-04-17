using Commander.UI;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using static CsTools.Extensions.Core;

namespace Commander.Controllers;

class RemotesController : ControllerBase<RemotesItem>, IController
{
    #region IController

    public string CurrentPath { get; private set; } = "remotes";

    public int Directories { get; private set; }

    public int Files { get; }

    public int HiddenDirectories => 0;

    public int HiddenFiles => 0;

    public bool CheckRestriction(string searchKey) => false;

    public Task CopyItems(string? targetPath, bool move) => Unit.Value.ToAsync();

    public Task CreateFolder() => Unit.Value.ToAsync();

    public async Task DeleteItems()
    {
        // var item = GetItem(GetFocusedItemPos());
        // if (item != null && item.Path != "")
        // {
        //     var response = await AlertDialog.PresentAsync("Löschen?", "Möchtest du den Favoriten löschen?");
        //     if (response == "ok")
        //         Storage.DeleteFavorite(item);
        // }
    }

    public Task<int> Fill(string path, FolderView folderView)
    {
        var home = new RemotesItem(
            "..",
            "");
        var newRemote = new RemotesItem(
            "Entferntes Gerät hinzufügen...",
            "");

        //var items = ConcatEnumerables([home], Storage.Retrieve().Favorites ?? [], [newRemote]).ToArray();
        var items = ConcatEnumerables([home], [newRemote]).ToArray();        

        Insert(items);
        Directories = items.Length;
        return (-1).ToAsync();
    }

    public ExifData? GetExifData(int pos) => null;

    public string? GetItemPath(int pos) => null;

    public async Task<string?> OnActivate(int pos)
    {
        var item = GetItem(pos);
        if (item != null && item.Name == "..")
            return "root";
        else if (item != null && item.Path != "")
            return item.Path;
        else
        {
            var inactive = FolderViewPaned.Instance.GetInactiveFolderView();
            if (inactive != null)
            {
                var newName = await AddRemote.ShowAsync();
                // if (newName != null)
                //     Storage.SaveFavorite(new(newName, inactive.Context.CurrentPath));
                FolderViewPaned.Instance.GetActiveFolderView()?.Refresh();
            }
            return null;
        }
    }

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
    {
        model.UnselectRange(pos, count);
        return 0;
    }

    public async Task Rename()
    {
        // var item = GetItem(GetFocusedItemPos());
        // if (item != null && item.Path != "")
        // {
        //     var newName = await TextDialog.ShowAsync("Umbennen?", "Möchtest du den Favoriten umbenennen?", item.Name);
        //     if (newName != null)
        //         Storage.ChangeFavorite(item, newName);
        // }
    }

    public void SelectAll(FolderView folderView) { }
    public void SelectCurrent(FolderView folderView) { }
    public void SelectNone(FolderView folderView) { }
    public void SelectToEnd(FolderView folderView) { }
    public void SelectToStart(FolderView folderView) { }

    #endregion

    public RemotesController(FolderView folderView) : base()
        => folderView.SetController(this);

    public override ColumnViewSubClassed.Column<RemotesItem>[] GetColumns()
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

    void OnIconNameBind(ListItemHandle listItem, RemotesItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.Name == ".."
            ? "go-up"
            : item.Path == ""
            ? "list-add"
            : "starred";
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
    }
}

record RemotesItem(string Name, string Path);
    