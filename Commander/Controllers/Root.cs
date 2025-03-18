using CsTools.Async;
using CsTools.Extensions;

using static GtkDotNet.Controls.ColumnViewSubClassed;
using static CsTools.ProcessCmd;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using Commander.UI;

namespace Commander.Controllers;

class Root() : Controller<RootItem>, IController
{
    #region IController

    public void Set(FolderView folderView)
        => folderView.SetController(this);

    public async void Fill()
    {
        // TODO Fill sda when there is no sda1 (daten)
        var rootItems = await
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
              select item));

        Insert(rootItems);
    }

    public string? OnActivate(uint pos)
    {
        // TODO when not mounted, mount
        var item = GetItem(pos);
        Console.WriteLine($"Eintrag ausgewählt: {item}");
        return item?.MountPoint;
    }

    #endregion

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
                OnLabelBind = i => i.Size.ToString()
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
                DriveType.Home)
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
                    "ext4" => DriveType.Ext4,
                    "ntfs" => DriveType.Ntfs,
                    "vfat" => DriveType.Vfat,
                    _ => DriveType.Unknown
                }
            );
        string GetString(int pos1, int pos2)
            => driveString[columnPositions[pos1]..columnPositions[pos2]].Trim();
    }

    static bool FilterDrives(string driveString, int[] columnPositions) =>
        driveString == "home"
        || driveString[columnPositions[1]] > '~';

    static void OnIconNameBind(ListItemHandle listItem, RootItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.DriveType switch
        {
            DriveType.Home => "user-home",
            _ => "drive-removable-media-symbolic"
        };
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
        if (string.IsNullOrEmpty(item.MountPoint))
            box?.GetParent<WidgetHandle>()?.GetParent().AddCssClass("hiddenItem");  // TODO AddCssClass(cls, bool add)
        else
            box?.GetParent<WidgetHandle>()?.GetParent().RemoveCssClass("hiddenItem");
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
    DriveType DriveType);

enum DriveType
{
    Unknown,
    Ext4,
    Ntfs,
    Vfat,
    Home
}
