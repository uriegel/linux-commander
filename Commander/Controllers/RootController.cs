using CsTools.Extensions;
using GtkDotNet;

using static CsTools.ProcessCmd;
using static CsTools.Extensions.Core;

namespace Commander.Controllers;

class RootController : Controller
{
    public override string Id { get; } = "ROOT";

    public override async Task<ChangePathResult> ChangePathAsync(string path)
    {
        var rootItems = await GetRootItems();
        var mounted = rootItems.Where(n => n.IsMounted);
        var unmounted = rootItems.Where(n => !n.IsMounted);

        var home = new RootItem(
            "~",
            "home",
            -1,
            CsTools.Directory.GetHomeDir(),
            true,
            false,
            DriveKind.Home);
        var fav = new RootItem(
            "fav",
            "Favoriten",
            -1,
            "fav",
            true,
            false,
            DriveKind.Unknown);
        var remotes = new RootItem(
            "remotes",
            "Zugriff auf entfernte Geräte",
            -1,
            "remotes",
            true,
            false,
            DriveKind.Unknown);

        var items = ConcatEnumerables([home], mounted, [fav, remotes], unmounted).ToArray();
        return new RootResult(CheckInitial() ? Id : null, "root", items.Length, 0, items);
    }

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
}

static partial class Extensions
{
    public static string TrimName(this string name)
        => name.Length > 2 && name[1] == '─'
        ? name[2..]
        : name;
}

enum DriveKind
{
    Unknown,
    Ext4,
    Ntfs,
    Vfat,
    Home,
}

record RootResult(
    string? Controller,
    string Path,
    int DirCount,
    int FileCount,
    RootItem[] Items
)
    : ChangePathResult(Controller, Path, DirCount, FileCount);


record RootItemOffer(RootItem RootItem, bool IsRoot);

record RootItem(
    string Name,
    string Description,
    long? Size,
    string? MountPoint,
    bool IsMounted,
    bool IsEjectable,
    DriveKind DriveKind
)
    : ViewItem(Name, Size, null, null);

