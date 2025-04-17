using CsTools.Async;
using CsTools.Extensions;

using GtkDotNet;
using GtkDotNet.SafeHandles;
using Commander.Enums;
using Commander.UI;

using static GtkDotNet.Controls.ColumnViewSubClassed;
using static CsTools.ProcessCmd;
using CsTools;

using static CsTools.Extensions.Core;
using System.Data;
using Commander.DataContexts;

namespace Commander.Controllers;

class RootController : ControllerBase<RootItem>, IController
{
    #region IController

    public string CurrentPath { get; } = "root";

    public int Directories { get; private set; }
    public int Files { get; }

    public int HiddenDirectories { get; } = 0;
    public int HiddenFiles { get; } = 0;

    public string? GetItemPath(int pos)
    {
        var item = GetItem(pos);
        return !string.IsNullOrEmpty(item?.Description) ? item?.Description : item?.Name;
    }
     
    public ExifData? GetExifData(int pos) => null;

    public async Task<int> Fill(string path, FolderView folderView)
    {
        var rootItems = await GetRootItems();
        var mounted = rootItems.Where(n => n.IsMounted);
        var unmounted = rootItems.Where(n => !n.IsMounted);

        var home = new RootItem(
            "~",
            "home",
            0,
            CsTools.Directory.GetHomeDir(),
            true,
            false,
            DriveKind.Home);
        var fav = new RootItem(
            "fav",
            "Favoriten",
            0,
            "fav",
            true,
            false,
            DriveKind.Unknown);
        var remotes = new RootItem(
            "remotes",
            "Zugriff auf entfernte Geräte",
            0,
            "remotes",
            true,
            false,
            DriveKind.Unknown);

        var items = ConcatEnumerables([home], mounted, [fav, remotes], unmounted).ToArray();

        Insert(items);
        Directories = items.Length;
        return -1;
    }

    public async Task<string?> OnActivate(int pos)
    {
        var item = GetItem(pos);
        if (item != null && (item.Name == "fav" || item.Name == "remotes"))
            return item.Name;
        else if (item != null && string.IsNullOrWhiteSpace(item.MountPoint))
            return await MountAsync(item.Name);
        else
            return item?.MountPoint;
    }

    public bool CheckRestriction(string searchKey) => false;

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
    {
        model.UnselectRange(pos, count);
        return 0;
    }

    public void SelectAll(FolderView folderView) { }
    public void SelectNone(FolderView folderView) { }
    public void SelectCurrent(FolderView folderView) { }
    public void SelectToStart(FolderView folderView) { }
    public void SelectToEnd(FolderView folderView) { }

    #endregion

    public RootController(FolderView folderView) : base()
        => folderView.SetController(this);

    public override Column<RootItem>[] GetColumns()
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
                Title = "Bezeichnung",
                Expanded = true,
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End),
                OnLabelBind = i => i.Description
            },
            new()
            {
                Title = "Mountpoint",
                Expanded = true,
                Resizeable = true,
                OnLabelBind = i => i.DriveKind == DriveKind.Unknown ? "" : i.MountPoint ?? ""
            },
            new()
            {
                Title = "Größe",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != 0 ? i.Size.ToString() : ""
            }];

    public async Task DeleteItems()
    {
        var pos = GetFocusedItemPos();
        var focusedItem = Items().Skip(pos).FirstOrDefault();
        if (focusedItem?.IsEjectable == true)
        {
            var volumes = VolumeMonitor.Get().GetVolumes();
            var device = volumes.FirstOrDefault(n => n.GetUnixDevice()?.EndsWith(focusedItem?.Name ?? "---") == true);
            if (device != null && await AlertDialog.PresentAsync("Auswerfen?", "Möchtest du das Laufwerk auswerfen?") == "ok")
            {
                try
                {
                    using var mo = MountOperation.New();
                    await device.EjectAsync(UnmountFlags.Force, mo);
                }
                catch (Exception e)
                {
                    MainContext.Instance.ErrorText = "Das Laufwerk konnte nicht ausgeworfen werden";
                    Console.WriteLine($"Konnte {focusedItem?.Name} nicht auswerfen: {e}");
                }
            }
        }
    }

    public Task Rename() => Unit.Value.ToAsync();
    public Task CreateFolder() => Unit.Value.ToAsync();
    public Task CopyItems(string? _, bool __) => Unit.Value.ToAsync();

    async Task<RootItem[]> GetRootItems()
    {
        var lsblkResult = await RunAsync("lsblk", "--bytes --output SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE");
        var driveLines = lsblkResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var titles = driveLines[0];
        var columnPositions = new[]
        {
            0,
            titles.IndexOf("NAME"),
            titles.IndexOf("LABEL"),
            titles.IndexOf("MOUNT"),
            titles.IndexOf("FSTYPE")
        };

        var rootItemsOffers =
            driveLines
                .Skip(1)
                .Select(n => CreateRootItem(n, columnPositions))
                .ToArray();

        return rootItemsOffers
            .Where(FilterDrives)
            .Select(n => n.RootItem)
            .OrderBy(n => n.Name)
            .ToArray();

        bool FilterDrives(RootItemOffer rio)
            => !rio.IsRoot
                || (!rootItemsOffers.Any(n => n.RootItem.Name != rio.RootItem.Name && n.RootItem.Name.StartsWith(rio.RootItem.Name))
                    && rio.RootItem.MountPoint != "[SWAP]");
    }

    static RootItemOffer CreateRootItem(string driveString, int[] columnPositions)
    {
        var volumes = VolumeMonitor.Get().GetVolumes();
        var name = GetString(1, 2).TrimName();
        var mountPoint = GetString(3, 4);
        return new(new(
                name,
                GetString(2, 3),
                GetString(0, 1)
                    .ParseLong()
                    ?? 0,
                mountPoint,
                mountPoint.Length > 0,
                volumes.FirstOrDefault(n => n.GetUnixDevice()?.EndsWith(name) == true)?.CanEject() == true,
                driveString[columnPositions[4]..].Trim() switch
                {
                    "ext4" => DriveKind.Ext4,
                    "ntfs" => DriveKind.Ntfs,
                    "vfat" => DriveKind.Vfat,
                    _ => DriveKind.Unknown
                }
            ), driveString[columnPositions[1]] < '~');

        string GetString(int pos1, int pos2)
            => driveString[columnPositions[pos1]..columnPositions[pos2]].Trim();
    }

    static async Task<string> MountAsync(string path)
    {
        var result = await RunAsync("udisksctl", $"mount -b /dev/{path}");
        return result?.SubstringAfter(" at ").Trim() ?? path;
    }

    void OnIconNameBind(ListItemHandle listItem, RootItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.DriveKind switch
        {
            DriveKind.Home => "user-home",
            //_ => "drive-removable-media-symbolic"
            DriveKind.Unknown when item.Name == "fav" => "starred",
            DriveKind.Unknown when item.Name == "remotes" => "network-server",
            _ => item.IsEjectable ? "media-removable" : "drive-removable-media"
        };
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
        box?.GetParent<WidgetHandle>()?.GetParent().AddCssClass("hiddenItem", !item.IsMounted);
    }
}

static partial class Extensions
{
    public static string TrimName(this string name)
        => name.Length > 2 && name[1] == '─'
        ? name[2..]
        : name;
}

record RootItem(
    string Name,
    string Description,
    long Size,
    string? MountPoint,
    bool IsMounted,
    bool IsEjectable,
    DriveKind DriveKind);

record RootItemOffer(RootItem RootItem, bool IsRoot);