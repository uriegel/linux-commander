using Commander.Settings;
using Commander.UI;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

using static CsTools.Extensions.Core;

namespace Commander.Controllers;

class RemotesController : ControllerBase<RemoteItem>, IController
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
        var item = GetItem(GetFocusedItemPos());
        if (item != null && item.IP != "")
        {
            var response = await AlertDialog.PresentAsync("Entfernen?", "Möchtest du das entfernte Gerät entfernen?");
            if (response == "ok")
                Storage.DeleteRemote(item);
        }
    }

    public Task<int> Fill(string path, FolderView folderView)
    {
        var home = new RemoteItem(
            "..",
            "",
            false);
        var newRemote = new RemoteItem(
            "Entferntes Gerät hinzufügen...",
            "",
            false);

        var items = ConcatEnumerables([home], Storage.Retrieve().Remotes ?? [], [newRemote]).ToArray();
        
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
        else if (item != null && item.IP != "")
            return item.IP;
        else
        {
            var inactive = FolderViewPaned.Instance.GetInactiveFolderView();
            if (inactive != null)
            {
                var newItem = await AddRemoteDialog.ShowAsync();
                if (newItem != null)
                    Storage.SaveRemote(newItem);
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
        var item = GetItem(GetFocusedItemPos());
        if (item != null && item.IP != "")
        {
            var newName = await TextDialog.ShowAsync("Umbennen?", "Möchtest du das entfernte Gerät umbenennen?", item.Name);
            if (newName != null)
                Storage.ChangeRemote(item, newName);
        }
    }

    public void SelectAll(FolderView folderView) { }
    public void SelectCurrent(FolderView folderView) { }
    public void SelectNone(FolderView folderView) { }
    public void SelectToEnd(FolderView folderView) { }
    public void SelectToStart(FolderView folderView) { }

    #endregion

    public RemotesController(FolderView folderView) : base()
        => folderView.SetController(this);

    public override ColumnViewSubClassed.Column<RemoteItem>[] GetColumns()
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
                Title = "IP-Adresse",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End),
                OnLabelBind = i => i.IP
            }
        ];

    void OnIconNameBind(ListItemHandle listItem, RemoteItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.Name == ".."
            ? "go-up"
            : item.IP == ""
            ? "list-add"
            : item.IsAndroid
            ? "phone"
            : "network-server";
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
    }
}

record RemoteItem(string Name, string IP, bool IsAndroid);
    