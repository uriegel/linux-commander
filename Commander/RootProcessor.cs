using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

static class RootProcessor
{
    public record RootItem(string Name, string Display, string MountPoint, long Capacity, string FileSystem);

    public static async Task<IEnumerable<RootItem>> GetItemsAsync()
    {
        var response = await Process.RunAsync("lsblk", "--bytes --output SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE");
        var lines = response.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        var columnPositions = new[]
        {
            0,
            GetPart("NAME"),
            GetPart("LABEL"),
            GetPart("MOUNT"),
            GetPart("FSTYPE")
        };

        return new RootItem[] { new("~", "home", Environment.GetFolderPath(Environment.SpecialFolder.Personal), 0, "") }
            .Concat(lines
                .Skip(1)
                .Where(FilterDrives)
                .Select(GetItem)
            );

        int GetPart(string key) => lines[0].IndexOf(key);

        string GetStringPart(string line, int start, int? end = null)
        {
            var startIndex = columnPositions[start];
            var endIndex = end.HasValue ? columnPositions[end.Value] as int? : null;
            return (endIndex.HasValue ? line[startIndex..endIndex.Value] : line[startIndex..]).TrimEnd();
        }

        bool FilterDrives(string line) => line[columnPositions[1]] > '~';

        RootItem GetItem(string line)
            => new RootItem(
                GetStringPart(line, 1, 2)[2..],
                GetStringPart(line, 2, 3),
                GetStringPart(line, 3, 4),
                GetStringPart(line, 0, 1).ParseLong(),
                GetStringPart(line, 4)
            );
    }
}