using CsTools.Async;
using CsTools.Extensions;

using GtkDotNet;
using GtkDotNet.SafeHandles;
using Commander.Enums;
using Commander.UI;

using static GtkDotNet.Controls.ColumnViewSubClassed;
using static CsTools.ProcessCmd;

namespace Commander.Controllers;

class RootController : Controller<RootItem>, IController
{
    #region IController

    public string CurrentPath { get; } = "root";

    public int Directories { get; private set; }
    public int Files { get; } 

    public string? GetItemPath(int pos) => GetItem(pos)?.MountPoint;

    public async Task<int> Fill(string path)
    {
        // TODO Fill sda when there is no sda1 (daten)
        var rootItems = (await
        (from n in RunAsync("lsblk", "--bytes --output SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE")
         let driveLines = n.Split('\n', StringSplitOptions.RemoveEmptyEntries)
         let titles = driveLines[0]
         let columnPositions = new[]
         {
                0,
                titles.IndexOf("NAME"),
                titles.IndexOf("LABEL"),
                titles.IndexOf("MOUNT"),
                titles.IndexOf("FSTYPE")
            }
         select
             (from n in driveLines
                 .Skip(1)
                 .Append("home")
              where FilterDrives(n, columnPositions)
              let item = CreateRootItem(n, columnPositions)
              orderby item.IsMounted descending, item.Name
              select item)))
            .ToArray();

        Insert(rootItems);
        Directories = rootItems.Length;
        return -1;
    }

    public string? OnActivate(int pos)
    {
        // TODO when not mounted, mount
        var item = GetItem(pos);
        return item?.MountPoint;
    }

    public void OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl) => model.UnselectRange(pos, count);

    public void SelectAll(FolderView folderView) {}
    public void SelectNone(FolderView folderView) {}
    public void SelectCurrent(FolderView folderView) {}
    public void SelectToStart(FolderView folderView) {}
    public void SelectToEnd(FolderView folderView) {}

    #endregion

    public RootController(FolderView folderView)
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
                OnLabelBind = i => i.MountPoint
            },
            new()
            {
                Title = "Größe",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != 0 ? i.Size.ToString() : ""
            }];

    RootItem CreateRootItem(string driveString, int[] columnPositions)
    {
        var mountPoint = driveString != "home"
            ? GetString(3, 4)
            : "";

        return driveString == "home"
            ? new(
                "~",
                "home",
                0,
                CsTools.Directory.GetHomeDir(),
                true,
                DriveKind.Home)
            : new(
                GetString(1, 2).TrimName(),
                GetString(2, 3),
                GetString(0, 1)
                    .ParseLong()
                    ?? 0,
                mountPoint,
                mountPoint.Length > 0,
                driveString[columnPositions[4]..].Trim() switch
                {
                    "ext4" => DriveKind.Ext4,
                    "ntfs" => DriveKind.Ntfs,
                    "vfat" => DriveKind.Vfat,
                    _ => DriveKind.Unknown
                }
            );
        string GetString(int pos1, int pos2)
            => driveString[columnPositions[pos1]..columnPositions[pos2]].Trim();
    }

    static bool FilterDrives(string driveString, int[] columnPositions) =>
        driveString == "home"
        || driveString[columnPositions[1]] > '~';

    void OnIconNameBind(ListItemHandle listItem, RootItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.DriveKind switch
        {
            DriveKind.Home => "user-home",
            //_ => "drive-removable-media-symbolic"
            _ => "drive-removable-media"
        };
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
        box?.GetParent<WidgetHandle>()?.GetParent().AddCssClass("hiddenItem", string.IsNullOrEmpty(item.MountPoint));
    }
}

static class Extensions
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
    string MountPoint,
    bool IsMounted,
    DriveKind DriveKind);

