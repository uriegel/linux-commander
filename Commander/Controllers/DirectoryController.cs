using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using Commander.DataContexts;
using Commander.Enums;
using Commander.UI;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;

using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

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
        var items = result.Items.Select(n => n.CloneItem()).ToArray(); 
        Insert(items);
        StartExifResolving(items, folderView);
        Directories = result.DirCount;
        Files = result.FileCount;
        HiddenDirectories = result.HiddenDirCount;
        HiddenFiles = result.HiddenFileCount;
        return FindPos(n => n.Name == oldPath.SubstringAfterLast('/'));
    }

    public async Task<bool> DeleteItems()
    {
        var type = GetSelectedItemsType(GetFocusedItemPos());
        if (type == SelectedItemsType.None)
            return false;
        var text = type switch
        {
            SelectedItemsType.Both => "Möchtest Du die markierten Einträge löschen?",
            SelectedItemsType.Files => "Möchtest Du die markierten Dateien löschen?",
            SelectedItemsType.Folders => "Möchtest Du die markierten Verzeichnisse löschen?",
            SelectedItemsType.File => "Möchtest Du die markierte Datei löschen?",
            SelectedItemsType.Folder => "Möchtest Du das markierte Verzeichnis löschen?",
            _ => ""
        };
        var response = await AlertDialog.PresentAsync("Löschen?", text);
        if (response == "ok")
        {
            foreach (var item in GetSelectedItems(GetFocusedItemPos()))
            {
                using var file = GFile.New(CurrentPath.AppendPath(item.Name));
                await file.TrashAsync();
            }
            return true;
        }
        else
            return false;
    }

    public async Task<bool> Rename()
    {
        var type = GetSelectedItemsType(GetFocusedItemPos());
        if (type == SelectedItemsType.None)
            return false;
        var text = type switch
        {
            SelectedItemsType.File => "Möchtest Du die markierte Datei umbenennen?",
            SelectedItemsType.Folder => "Möchtest Du das markierte Verzeichnis umbenennen?",
            _ => null
        };
        if (text == null)
            return false;
        var item = GetSelectedItems(GetFocusedItemPos()).FirstOrDefault();
        var newFile = await TextDialog.ShowAsync("Umbennen?", text, item?.Name, () => (0, item?.Name?.LastIndexOf('.') ?? 0));
        if (newFile != null && item != null && !string.IsNullOrWhiteSpace(text))
        {
            if (item.IsDirectory)
                Directory.Move(CurrentPath.AppendPath(item.Name), CurrentPath.AppendPath(newFile));
            else
                File.Move(CurrentPath.AppendPath(item.Name), CurrentPath.AppendPath(newFile));
            return true;
        }
        return false;
    }

    public async Task<bool> ExtendedRename(FolderView folderView)
    {
        var res = await ExtendedRenameDialog.ShowAsync();
        if (res != null && extendedRename == null)
        {
            folderView.InsertColumn<DirectoryItem>(1, new()
            {
                Title = "Neuer Name",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End).MarginEnd(3),
                OnItemBind = (l, item) =>
                {
                    var label = l.GetChild<LabelHandle>();
                    item.RenameLabel = label;
                    label.Set(item.RenameName ?? "");
                    item.PropertyChanged += RenameNameUpdate;
                    //Console.WriteLine($"Labelbind: {item.Name}");
                },
                OnItemUnbind = (l, item) =>
                {
                    var label = l.GetChild<LabelHandle>();
                    item.RenameLabel = null;
                    item.PropertyChanged -= RenameNameUpdate;
                    //Console.WriteLine($"Label unbind: {item.Name}");
                }
            });
        }
        if (res == null && extendedRename != null)
            folderView.RemoveCol(1);
        extendedRename = res;
        SetExtendedRenameNames();
        return false;
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
            if (item != null && !string.IsNullOrWhiteSpace(text))
            {
                Directory.CreateDirectory(CurrentPath.AppendPath(newFile));
                return true;
            }
        }
        return false;
    }

    public Task<bool> CopyItems(string? targetPath, bool move)
    {
        if (targetPath?.StartsWith("remote/") != true)
        {
            var copyProcessor = new CopyProcessor(CurrentPath, targetPath,
                GetSelectedItemsType(GetFocusedItemPos()), [.. GetSelectedItems(GetFocusedItemPos())]);
            return copyProcessor.CopyItems(move);
        }
        else if (!move)
        {
            var copyProcessor = new CopyToRemoteProcessor(CurrentPath, targetPath,
                GetSelectedItemsType(GetFocusedItemPos()), [.. GetSelectedItems(GetFocusedItemPos())]);
            return copyProcessor.CopyItems(false);
        }
        else
            return false.ToAsync();
    }

    public async Task<string?> OnActivate(int pos)
    {
        if (extendedRename != null && pos > 0 && GetSelectedItems().Any() && MainContext.Instance.Restriction == null)
        {
            await RunExtendedRename();
            return null;
        }
        var item = GetItem(pos);
        if (item != null && (item.Kind == ItemKind.Folder || item.Kind == ItemKind.Parent))
            if (CurrentPath != "/" || item.Kind != ItemKind.Parent)
                return Path.Combine(CurrentPath, item.Name);
            else
                return "root";
        else if (item != null && item.Kind == ItemKind.Item)
        {
            StartItem(item.Name);
            return null;
        }
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
        SetExtendedRenameNames();
        return GetSelectedItemsIndexes().Count();
    }

    public void SelectAll(FolderView folderView) => folderView.SelectAll();
    public void SelectNone(FolderView folderView) => folderView.UnselectAll();
    public void SelectCurrent(FolderView folderView) => folderView.SelectCurrent();
    public void SelectToStart(FolderView folderView) => folderView.SelectToStart();
    public void SelectToEnd(FolderView folderView) => folderView.SelectToEnd();

    public async void OnSortChanging()
    {
        await Task.Delay(500);
        SetExtendedRenameNames();
    }

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
                OnItemSetup = () => Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End),
                OnItemBind = (listItem, item) =>
                {
                    item.TimeLabel = listItem.GetChild<LabelHandle>();
                    OnTimeBind(item.TimeLabel, item);
                    item.PropertyChanged += TimeUpdate;
                },
                OnItemUnbind = (listItem, item) =>
                {
                    item.TimeLabel = null;
                    item.PropertyChanged -= TimeUpdate;
                }
            },
            new()
            {
                Title = "Größe",
                OnSort = OnSizeSort,
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != -1 ? i.Size.ToString("N0") : ""
            }
        ];

    public SelectedItemsType GetSelectedItemsType(int focusedItemPos)
    {
        var selItems = GetSelectedItems();
        var dirs = selItems.Count(n => n.Kind == ItemKind.Folder);
        var files = selItems.Count(n => n.Kind == ItemKind.Item);
        var result = dirs > 1 && files == 0
            ? SelectedItemsType.Folders
            : dirs == 0 && files > 1
            ? SelectedItemsType.Files
            : dirs == 1 && files == 0
            ? SelectedItemsType.Folder
            : dirs == 0 && files == 1
            ? SelectedItemsType.File
            : dirs + files > 0
            ? SelectedItemsType.Both
            : SelectedItemsType.None;
        if (result != SelectedItemsType.None)
            return result;
        var focusedItem = Items().Skip(focusedItemPos).FirstOrDefault();
        return focusedItem?.Kind switch
        {
            ItemKind.Folder => SelectedItemsType.Folder,
            ItemKind.Item => SelectedItemsType.File,
            _ => SelectedItemsType.None
        };
    }

    public static string GetIconName(string fileName)
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

    public static int OnNameSort(DirectoryItem a, DirectoryItem b, bool desc)
        => OnFolderOrParentSort(a, b, desc) ?? string.Compare(a.Name, b.Name);

    public static int OnTimeSort(DirectoryItem a, DirectoryItem b, bool desc)
        => OnFolderOrParentSort(a, b, desc)
            ?? (a.GetDateTime().HasValue && b.GetDateTime().HasValue
            ? (a.GetDateTime()!.Value - b.GetDateTime()!.Value).TotalMilliseconds > 0
            ? 1
            : (a.GetDateTime()!.Value - b.GetDateTime()!.Value).TotalMilliseconds < 0
            ? -1
            : 0
            : 0);

    public static int OnSizeSort(DirectoryItem a, DirectoryItem b, bool desc)
        => OnFolderOrParentSort(a, b, desc)
            ?? (a.Size > b.Size
            ? 1
            : a.Size < b.Size
            ? -1
            : 0);

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
                folderView.InvalidateFocus();
            }
        });
    }

    static void RenameNameUpdate(object? obj, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DirectoryItem.RenameName) && obj is DirectoryItem item)
            item?.RenameLabel?.Set(item.RenameName ?? "");
    }

    async Task RunExtendedRename()
    {
        var renameItems = GetSelectedItems().ToArray();
        if (renameItems.Length > 0 && await AlertDialog.PresentAsync("Umbenennen", "Erweiterte Umbenennungen starten?") == "ok")
        {

            var selectedIndexes = GetSelectedItemsIndexes().ToImmutableDictionary(n => n);
            var items = Items()
                            .Select((n, i) => (n, i))
                            .Where(n => n.n.IsDirectory == false)
                            .Select(n => selectedIndexes.ContainsKey(n.i) ? n.n.RenameName ?? "" : n.n.Name ?? "");
            if (items.ToImmutableDictionary(n => n).Count == items.Count())
            {

            }
            else
                MainContext.Instance.ErrorText = "Dateinamen nicht eindeutig";
        }
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

    static void TimeUpdate(object? obj, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DirectoryItem.ExifData) && obj is DirectoryItem item && item.TimeLabel != null)
            OnTimeBind(item.TimeLabel, item);
    }

    static void OnTimeBind(LabelHandle label, DirectoryItem item)
    {
        label.Set(
            item.ExifData?.DateTime.HasValue == true
            ? item.ExifData.DateTime.Value.ToString()?[..^3]
            : (item.Time.HasValue ? item.Time.Value.ToString()?[..^3] : ""));
        label.AddCssClass("exif", item.ExifData?.DateTime != null);
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

    static bool Filter(DirectoryItem item)
        => (!item.IsHidden || Actions.Instance.ShowHidden)
            && (MainContext.Instance.Restriction == null
                || item.Name.StartsWith(MainContext.Instance.Restriction, StringComparison.CurrentCultureIgnoreCase));

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

    void StartItem(string name)
    {
        using var proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "xdg-open",
                Arguments = $"\"{CurrentPath.AppendPath(name)}\"",
            },
        };
        proc.Start();
    }

    void SetExtendedRenameNames()
    {
        if (extendedRename != null)
        {
            foreach (var item in Items().Where(n => !n.IsDirectory))
                item.RenameName = null;
            var index = extendedRename.StartIndex;
            foreach (var selItem in GetSelectedItems().Where(n => !n.IsDirectory))
                selItem.RenameName = $"{extendedRename.Prefix}{index++.ToString().PadLeft(extendedRename.Digits, '0')}{selItem.Name.GetFileExtension()}";
        }
    }

    CancellationTokenSource cancellation = new();

    ExtendedRenameData? extendedRename;

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
