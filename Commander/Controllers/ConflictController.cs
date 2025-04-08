using Commander.UI;
using GtkDotNet;
using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

record ConflictItem(long Size);

class ConflichtController : Controller<ConflictItem>
{
    #region IController

    public override Column<ConflictItem>[] GetColumns() =>
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
                //OnItemBind = OnIconNameBind
            },
            new()
            {
                Title = "Datum",
                Resizeable = true,
                OnItemSetup = () => Label.New(),
                //OnItemBind = OnTimeBind
            },
            new()
            {
                Title = "Größe",
                Resizeable = true,
                OnItemSetup = () => Label.New().HAlign(Align.End).MarginEnd(3),
                OnLabelBind = i => i.Size != -1 ? i.Size.ToString() : ""
            }
        ];


    #endregion

    public ConflichtController(ConflictView conflictView) : base()
        => conflictView.SetController(this);
}
