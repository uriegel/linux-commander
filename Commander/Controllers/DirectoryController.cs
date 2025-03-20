using System.ComponentModel;
using Commander.Enums;
using Commander.UI;
using GtkDotNet;
using GtkDotNet.SafeHandles;

using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

// TODO sort parent -> folder by name -> item by sort
// TODO SortModel detect descending /ascending, but only item sort to outside
// TODO File Icons

class DirectoryController : Controller<DirectoryItem>, IController, IDisposable
{
    #region IController

    public async void Fill(string path)
    {
        var result = await Task.Factory.StartNew(() => DirectoryProcessing.GetFiles(path));
        RemoveAll();
        Insert(result.Items);
    }

    public string? OnActivate(uint pos)
    {
        return null;
    }

    #endregion

    public DirectoryController(FolderView folderView)
    {
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
                OnItemSetup = () => Label.New(),
                OnLabelBind = i => i.Time.ToString() ?? ""
            },
            new()
            {
                Title = "Größe",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != -1 ? i.Size.ToString() : ""
            }
        ];

    static void OnIconNameBind(ListItemHandle listItem, DirectoryItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = item.Kind switch
        {
            ItemKind.Parent => "go-up",
            ItemKind.Folder => "folder-open",
            _ => "text-x-generic-template"
        };
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Name);
        label?.Set(item.Name);
        if (item.IsHidden)
            box?.GetParent<WidgetHandle>()?.GetParent().AddCssClass("hiddenItem");  // TODO AddCssClass(cls, bool add)
        else
            box?.GetParent<WidgetHandle>()?.GetParent().RemoveCssClass("hiddenItem");
    }

    bool Filter(DirectoryItem item)
        => !item.IsHidden || Actions.Instance.ShowHidden;

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
