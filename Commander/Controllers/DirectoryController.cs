using Commander.DataContexts;
using Commander.Enums;
using Commander.UI;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;

using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

// TODO Menu actions like adapt view
// TODO Backspace history

// TODO overlay show errors in red

// TODO Pdf viewer: PdViewer in WebWindowNetCore
// TODO Pdf viewer: PdViewer in Gtk4DotNet
// TODO Pdf viewer
// TODO Text viewer/editor
// TODO Track viewer some inconsistencies like max velocity too high, trackpoints not containing data any more...

// TODO To Gtk4 await Task.Delay(1) to get a chance that datacontext is set on binding source;

class DirectoryController : ControllerBase<DirectoryItem>, IController, IDisposable
{
    #region IController

    public string CurrentPath { get; private set; } = "";

    public int Directories { get; private set; }
    public int Files { get; private set; }
    public int HiddenDirectories { get; private set; }
    public int HiddenFiles { get; private set; }

    public string? GetItemPath(int pos) => Path.Combine(CurrentPath, GetItem(pos)?.Name ?? "");

    public ExifData? GetExifData(int pos) => GetItem(pos)?.ExifData;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The position of the last path</returns>
    public async Task<int> Fill(string path, FolderView folderView)
    {
        cancellation.Cancel();
        cancellation = new();
        var result = await Task.Factory.StartNew(() => DirectoryProcessing.GetFiles(path));
        var oldPath = CurrentPath;
        CurrentPath = result.Path;
        StartExifResolving(result.Items, folderView);
        Insert(result.Items);
        Directories = result.DirCount;
        Files = result.FileCount;
        HiddenDirectories = result.HiddenDirCount;
        HiddenFiles = result.HiddenFileCount;
        return FindPos(n => n.Name == oldPath.SubstringAfterLast('/'));
    }

    public string? OnActivate(int pos)
    {
        var item = GetItem(pos);
        if (item != null && (item.Kind == ItemKind.Folder || item.Kind == ItemKind.Parent))
            if (CurrentPath != "/" || item.Kind != ItemKind.Parent)
                return Path.Combine(CurrentPath, item.Name);
            else
                return "root";
        else
            return null;
    }

    public bool CheckRestriction(string searchKey)
        => Items().Any(n => n.Name.StartsWith(searchKey, StringComparison.CurrentCultureIgnoreCase));

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
    {
        if (mouseButton && !mouseButtonCtrl && count == 1)
            model.UnselectRange(pos, 1);
        if (MainContext.Instance.Restriction == null)
            model.UnselectRange(0, 1);
        return GetSelectedItemsIndices().Count();
    }

    public void SelectAll(FolderView folderView) => folderView.SelectAll();
    public void SelectNone(FolderView folderView) => folderView.UnselectAll();
    public void SelectCurrent(FolderView folderView) => folderView.SelectCurrent();
    public void SelectToStart(FolderView folderView) => folderView.SelectToStart();
    public void SelectToEnd(FolderView folderView) => folderView.SelectToEnd();

    #endregion

    public DirectoryController(FolderView folderView) : base(folderView.GetSelectionModel())
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
                OnSort = OnNameSort,
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
                OnSort = OnTimeSort,
                OnItemSetup = () => Label.New(),
                OnItemBind = OnTimeBind
            },
            new()
            {
                Title = "Größe",
                OnSort = OnSizeSort,
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != -1 ? i.Size.ToString() : ""
            }
        ];

    void StartExifResolving(DirectoryItem[] items, FolderView folderView)
    {
        var token = cancellation.Token;
        Task.Run(() =>
        {
            folderView.Context.BackgroundAction = BackgroundAction.ExifDatas;
            try
            {
                foreach (var item in items
                                        .Where(item => !token.IsCancellationRequested
                                                && (item.Name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                                                    || item.Name.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                                                    || item.Name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))))
                    item.ExifData = ExifReader.GetExifData(CurrentPath.AppendPath(item.Name));
            }
            finally
            {
                folderView.Context.BackgroundAction = BackgroundAction.None;
                Gtk.Dispatch(() => folderView.Refresh());
            }
        });
    }

    static void OnIconNameBind(ListItemHandle listItem, DirectoryItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.Kind switch
        {
            ItemKind.Parent => "go-up",
            ItemKind.Folder => "folder-open",
            _ => GetIconName(item.Name)
        };
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
        box?.GetParent<WidgetHandle>()?.GetParent().AddCssClass("hiddenItem", item.IsHidden);
    }

    static void OnTimeBind(ListItemHandle listItem, DirectoryItem item)
    {
        var label = listItem.GetChild<LabelHandle>();
        label?.Set(item.ExifData?.DateTime.ToString() ?? item.Time.ToString() ?? "");
        label?.AddCssClass("exif", item.ExifData?.DateTime != null);
    }

    static string GetIconName(string fileName)
    {
        var ct = ContentType.Guess(fileName);
        if (ct != null)
        {
            using var icon = ContentType.GetIcon(ct);
            return icon
                    .ThemedNames()
                    .Where(Display.GetDefault().GetIconTheme().HasIcon) // TODO Skip(1), symbolic icons
                    .FirstOrDefault()
                 ?? "text-x-generic-template";
        }
        else
            return "text-x-generic-template";
    }


    static int? OnFolderOrParentSort(DirectoryItem a, DirectoryItem b, bool desc)
    {
        int? res = (a.Kind, b.Kind) switch
        {
            (ItemKind.Parent, _) => -1,
            (_, ItemKind.Parent) => 1,
            (ItemKind.Folder, ItemKind.Item) => -1,
            (ItemKind.Item, ItemKind.Folder) => 1,
            (ItemKind.Folder, ItemKind.Folder) => string.Compare(a.Name, b.Name),
            _ => null
        };
        return res != null
            ? desc
                ? -res
                : res
            : null;
    }

    bool Filter(DirectoryItem item)
        => (!item.IsHidden || Actions.Instance.ShowHidden)
            && (MainContext.Instance.Restriction == null
                || item.Name.StartsWith(MainContext.Instance.Restriction, StringComparison.CurrentCultureIgnoreCase));

    int OnNameSort(DirectoryItem a, DirectoryItem b, bool desc)
        => OnFolderOrParentSort(a, b, desc) ?? string.Compare(a.Name, b.Name);
    int OnTimeSort(DirectoryItem a, DirectoryItem b, bool desc)
        => OnFolderOrParentSort(a, b, desc)
            ?? (a.GetDateTime().HasValue && b.GetDateTime().HasValue
            ? (a.GetDateTime()!.Value - b.GetDateTime()!.Value).TotalMilliseconds > 0
            ? 1
            : (a.GetDateTime()!.Value - b.GetDateTime()!.Value).TotalMilliseconds < 0
            ? -1
            : 0
            : 0);

    int OnSizeSort(DirectoryItem a, DirectoryItem b, bool desc)
        => OnFolderOrParentSort(a, b, desc)
            ?? (a.Size > b.Size
            ? 1
            : a.Size < b.Size
            ? -1
            : 0);

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

    CancellationTokenSource cancellation = new();

    #region IDisposable

    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Verwalteten Zustand (verwaltete Objekte) bereinigen
                cancellation.Cancel();
                OnFilter = null;
            }

            // Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // Große Felder auf NULL setzen
            disposedValue = true;
        }
    }

    // Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~DirectoryController()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }

    bool disposedValue;

    #endregion
}
