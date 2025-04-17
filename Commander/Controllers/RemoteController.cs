using Commander.Enums;
using Commander.UI;
using CsTools;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using GtkDotNet;
using GtkDotNet.SafeHandles;

using static GtkDotNet.Controls.ColumnViewSubClassed;
using static CsTools.Extensions.Core;
using Commander.DataContexts;

namespace Commander.Controllers;

class RemoteController : ControllerBase<DirectoryItem>, IController
{
    #region IController

    public string CurrentPath { get; private set; } = "remote";

    public int Directories { get; private set; }

    public int Files { get; private set; }

    public int HiddenDirectories => 0;

    public int HiddenFiles => 0;

    public bool CheckRestriction(string searchKey) => false;

    public Task CopyItems(string? targetPath, bool move) => Unit.Value.ToAsync();

    public Task CreateFolder() => Unit.Value.ToAsync();

    public async Task DeleteItems()
    {
    }

    public async Task<int> Fill(string path, FolderView folderView)
    {
        var result = await path
            .GetIpAndPath()
            .Pipe(ipPath =>
                ipPath
                    .GetRequest()
                    .Get<RemoteItem[]>($"getfiles{ipPath.Path}", true)
                    .Select(n => n.Select(n => new DirectoryItem(
                        n.IsDirectory ? ItemKind.Folder : ItemKind.Item,
                        n.Name,
                        n.Size,
                        n.IsDirectory,
                        n.IsHidden,
                        n.Time.FromUnixTime()))))
            .SelectError(e => new RequestException(e))
            .GetOrThrowAsync();
        var oldPath = CurrentPath;
        CurrentPath = path;
        Insert(ConcatEnumerables([new DirectoryItem(ItemKind.Parent, "..", 0, true, false, null)], result));
        Directories = result.Where(n => n.IsDirectory).Count();
        Files = result.Where(n => !n.IsDirectory).Count();
        return FindPos(n => n.Name == oldPath.SubstringAfterLast('/'))
            .Minus1To0();
    }

    public ExifData? GetExifData(int pos) => null;

    public string? GetItemPath(int pos) => null;

    public Task<string?> OnActivate(int pos)
    {
        var item = GetItem(pos);
        if (item != null && (item.Kind == ItemKind.Folder || item.Kind == ItemKind.Parent))
            if (item.Kind != ItemKind.Parent)
                return ((string?)CurrentPath.CombineRemotePath(item.Name)).ToAsync();
            else
                return ((string?)CurrentPath.UpOne()).ToAsync();
        //return ((string?)"root").ToAsync();
        // else if (item != null && item.Kind == ItemKind.Item)
        // {
        //     StartItem(item.Name);
        //     return ((string?)null).ToAsync();
        // }
        else
            return ((string?)null).ToAsync();
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
    {
        EnableRubberband = true;
        folderView.SetController(this);
        OnFilter = Filter;
    }

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

    bool Filter(DirectoryItem item)
        => (!item.IsHidden || Actions.Instance.ShowHidden)
            && (MainContext.Instance.Restriction == null
                || item.Name.StartsWith(MainContext.Instance.Restriction, StringComparison.CurrentCultureIgnoreCase));

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
        label?.Set(item.Time.ToString() ?? "");
    }

    int FindPos(Func<DirectoryItem, bool> predicate)
    {
        var i = 0;
        while (true)
        {
            var item = GetItem(i);
            if (item == null)
                return -1;
            else
            {
                if (predicate(item))
                    return (int)i;
            }
            i++;
        }
    }
}

static partial class Extensions
{
    public static IpAndPath GetIpAndPath(this string url)
        => new(url.StringBetween('/', '/'),
            url[7..].Contains('/')
            ? "/" + url.SubstringAfter('/').SubstringAfter('/')
            : "");

    public static JsonRequest GetRequest(this IpAndPath ipAndPath)
        => new($"http://{ipAndPath.Ip}:8080");

    public static string CombineRemotePath(this string path, string subPath)
        => path.EndsWith('/')
            ? subPath.StartsWith('/')
                ? path + subPath[1..]
                : path + subPath
            : subPath.StartsWith('/')
                ? path + subPath
                : path + '/' + subPath;

    public static string UpOne(this string path)
        => path[7..].Contains('/')
            ? path.SubstringUntilLast('/')
            : "remotes";

    public static int Minus1To0(this int pos)
        => pos == -1
            ? 0
            : pos;
}

record RemoteItem(
    string Name,
    long Size,
    bool IsDirectory,
    bool IsHidden,
    long Time
);

record IpAndPath(string Ip, string Path);    