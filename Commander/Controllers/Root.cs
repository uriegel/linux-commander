using CsTools.Async;
using CsTools.Extensions;

using static GtkDotNet.Controls.ColumnViewSubClassed;
using static CsTools.ProcessCmd;
using Microsoft.VisualBasic;

namespace Controllers;

class Root() : Controller<RootItem>, IController
{
    #region IController

    public void Set(FolderView folderView)
        => folderView.SetController(this);

    public async void Fill()
    {
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

    #endregion

    public override Column<RootItem>[] GetColumns()
        => [ new()
                {
                    Title = "Name",
                    Expanded = true,
                    Resizeable = true,
                    OnLabelBind = i => i.Name
                    //OnItemSetup = OnIconName,
                    //OnItemBind = OnIconNameBind
        },
            new()
                {
                    Title = "Bezeichnung",
                    Expanded = true,
                    Resizeable = true,
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
                    OnLabelBind = i => i.Size.ToString()
            }
            ];

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
                "")
            : new(
                GetString(1, 2).TrimName(),
                GetString(2, 3),
                GetString(0, 1)
                    .ParseLong()
                    ?? 0,
                mountPoint,
                mountPoint.Length > 0,
                driveString[columnPositions[4]..]
                    .Trim()
            );
        string GetString(int pos1, int pos2)
            => driveString[columnPositions[pos1]..columnPositions[pos2]].Trim();
    }

    static bool FilterDrives(string driveString, int[] columnPositions) =>
        driveString == "home"
        || driveString[columnPositions[1]] > '~';
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
    string DriveType);

// class Controller : Controller<Type2>
// {
//     public Controller()
//     {
//         MultiSelection = true;
//         EnableRubberband = true;
//     }


//     public void Fill() => Insert([
//         .. Enumerable.Range(1, 100_000).Select(n => new Type2($"item{n}@dom.de", $"ID-{n}", n % 3 == 0))]);

//     static BoxHandle OnIconName()
//         => Box
//             .New(Orientation.Horizontal)
//             .Append(Image.NewFromIconName("mail", IconSize.Button))
//             .Append(Label.New("").HAlign(Align.Start).MarginStart(5));

//     static void OnIconNameBind(ListItemHandle listItem, Type2 item)
//     {
//         var box = listItem.GetChild<BoxHandle>();
//         var image = box.GetFirstChild<ImageHandle>();
//         var label = image.GetNextSibling<LabelHandle>();
//         if (item.Active)
//             image.SetFromIconName("mail-read", IconSize.LargeToolbar);
//         else
//             image.SetFromIconName("mail-unread", IconSize.LargeToolbar);
//         label.Set(item.EMail);
//         var itemHandle = listItem.GetItem<GObjectHandle>();
//         box.SetData("data", itemHandle.GetInternalHandle());
//     }
// }
