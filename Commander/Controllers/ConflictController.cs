using Commander.UI;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

class ConflichtController : Controller<CopyItem>
{
    #region IController

    public override Column<CopyItem>[] GetColumns() =>
        [
            new()
            {
                Title = "Name",
                Resizeable = true,
                Expanded = true,
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
                OnItemSetup = ()
                    => Box
                        .New(Orientation.Vertical)
                        .MarginEnd(10)
                        .Append(Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End))
                        .Append(Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End)),
                OnItemBind = OnTimeBind
            },
            new()
            {
                Title = "Größe",
                Resizeable = true,
                OnItemSetup = ()
                    => Box
                        .New(Orientation.Vertical)
                        .MarginEnd(5)
                        .Append(Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End))
                        .Append(Label.New().HAlign(Align.Start).Ellipsize(EllipsizeMode.End)),
                OnItemBind = OnSizeBind
            }
        ];


    #endregion

    public void Fill(IEnumerable<CopyItem> items) => Insert(items);

    static void OnIconNameBind(ListItemHandle listItem, CopyItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        var image = box?.GetFirstChild<ImageHandle>();
        var label = image?.GetNextSibling<LabelHandle>();
        var icon = DirectoryController.GetIconName(item.Source.Name);
        image?.SetFromIconName(icon, IconSize.Menu);
        label?.Set(item.Source.Name);
    }

    static void OnTimeBind(ListItemHandle listItem, CopyItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        box.AddCssClass("validated", item.Source.DateTime > item.Target?.DateTime);
        box.AddCssClass("not-validated", item.Source.DateTime < item.Target?.DateTime);
        var first = box.GetFirstChild<LabelHandle>();
        first.Set(item.Source.DateTime.ToString());
        var last = first.GetNextSibling<LabelHandle>();
        last.Set(item.Target?.DateTime.ToString());
    }

    static void OnSizeBind(ListItemHandle listItem, CopyItem item)
    {
        var box = listItem.GetChild<BoxHandle>();
        box.AddCssClass("different", item.Source.Size != item.Target?.Size);
        var first = box.GetFirstChild<LabelHandle>();
        first.Set(item.Source.Size.ToString());
        var last = first.GetNextSibling<LabelHandle>();
        last.Set(item.Target?.Size.ToString());
    }

    public ConflichtController(ConflictView conflictView) : base()
        => conflictView.SetController(this);
}
