using Commander.Enums;
using Commander.UI;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using Commander.DataContexts;

using static CsTools.HttpRequest.Core;
using static CsTools.Extensions.Core;
using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

// TODO Delete
// TODO Switch phone off (text)

class RemoteController : ControllerBase<DirectoryItem>, IController
{
    #region IController

    public string CurrentPath { get; private set; } = "remote";

    public int Directories { get; private set; }

    public int Files { get; private set; }

    public int HiddenDirectories => 0;

    public int HiddenFiles => 0;

    public bool CheckRestriction(string searchKey)
        => Items().Any(n => n.Name.StartsWith(searchKey, StringComparison.CurrentCultureIgnoreCase));

    public Task<bool> CopyItems(string? targetPath, bool move)
    {
        if (!move)
        {
            var copyProcessor = new CopyFromRemoteProcessor(CurrentPath, targetPath,
                GetSelectedItemsType(GetFocusedItemPos()), [.. GetSelectedItems(GetFocusedItemPos())]);
            return copyProcessor.CopyItems(move);
        }
        else
            return false.ToAsync();
    }

    public async Task<bool> CreateFolder()
    {
        var type = GetSelectedItemsType(GetFocusedItemPos());
        var text = "Möchtest Du einen neuen Ordner anlegen?";
        var builder = Builder.FromDotNetResource("textdialog");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        var textView = builder.GetWidget<EntryHandle>("text");
        dialog.Heading("Ordner anlegen?");
        dialog.Body(text);
        var item = GetSelectedItems(GetFocusedItemPos()).FirstOrDefault();
        textView.Text(item?.Kind == ItemKind.Folder ? item.Name : "");
        dialog.OnMap(textView.GrabFocus);

        var response = await dialog.PresentAsync(MainWindow.MainWindowHandle);
        if (response == "ok")
        {
            var newFile = textView.GetText();
            if (newFile != null && item != null && !string.IsNullOrWhiteSpace(text))
            {
                await Request
                    .Run(CurrentPath.CombineRemotePath(newFile).GetIpAndPath().PostCreateDirectory(), true)
                    .HttpGetOrThrowAsync();

                return true;
            }
        }
        return false;
    }

    public async Task<bool> DeleteItems()
    {
        return false;
    }

    public async Task<int> Fill(string path, FolderView folderView)
    {
        var result = await path
            .GetIpAndPath()
            .Pipe(ipPath =>
                ipPath
                    .GetRequest()
                    .Get<RemoteItem[]>($"getfiles{ipPath.Path}", true)
                    .Select(n => n
                        .OrderByDescending(n => n.IsDirectory)
                        .ThenBy(n => n.Name)
                        .Select(n => new DirectoryItem(
                            n.IsDirectory ? ItemKind.Folder : ItemKind.Item,
                            n.Name,
                            n.Size,
                            n.IsDirectory,
                            n.IsHidden,
                            n.Time.FromUnixTime()))))
            .HttpGetOrThrowAsync();
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
        if (mouseButton && !mouseButtonCtrl && count == 1)
            model.UnselectRange(pos, 1);

        if (selectionModel != null)
        {
            var dirs = selectionModel.GetItems<DirectoryItem>().Where(n => n.IsDirectory).Count();
            model.UnselectRange(0, dirs);
        }
        return GetSelectedItemsIndices().Count();
    }

    public async Task<bool> Rename()
    {
        return false;
    }

    public void SelectAll(FolderView folderView) => folderView.SelectAll();
    public void SelectNone(FolderView folderView) => folderView.UnselectAll();
    public void SelectCurrent(FolderView folderView) => folderView.SelectCurrent();
    public void SelectToStart(FolderView folderView) => folderView.SelectToStart();
    public void SelectToEnd(FolderView folderView) => folderView.SelectToEnd();

    #endregion

    public RemoteController(FolderView folderView) : base(folderView.GetSelectionModel())
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

    SelectedItemsType GetSelectedItemsType(int focusedItemPos)
    {
        var selItems = GetSelectedItems();
        var files = selItems.Count(n => n.Kind == ItemKind.Item);
        var result = files > 1
            ? SelectedItemsType.Files
            : files == 1
            ? SelectedItemsType.File
            : SelectedItemsType.None;
        if (result != SelectedItemsType.None)
            return result;
        var focusedItem = Items().Skip(focusedItemPos).FirstOrDefault();
        return focusedItem?.Kind switch
        {
            ItemKind.Item => SelectedItemsType.File,
            _ => SelectedItemsType.None
        };
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

    public static CsTools.HttpRequest.Settings PostCreateDirectory(this IpAndPath ipAndPath) 
        => DefaultSettings with
        {
            Method = HttpMethod.Post,
            BaseUrl = $"http://{ipAndPath.Ip}:8080",
            Url = $"/createdirectory/{ipAndPath.Path}",
        };    

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