using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Controllers;

class Root() : Controller<RootItem>
{
    public override Column<RootItem>[] GetColumns()
        => [ new()
                {
                    Title = "Name",
                    Expanded = true,
                    Resizeable = true,
                    // OnItemSetup = OnIconName,
            // OnItemBind = OnIconNameBind
        },
            new()
                {
                    Title = "Bezeichnung",
                    Expanded = true,
                    Resizeable = true,
                    //OnLabelBind = i => i.Id
            },
            new()
                {
                    Title = "Mountpoint",
                    Expanded = true,
                    Resizeable = true,
                    //OnLabelBind = i => i.Active ? "Yes" : "No",
            },
            new()
                {
                    Title = "Größe",
                    Resizeable = true,
                    //OnLabelBind = i => i.Active ? "Yes" : "No",
            }
            ];
}

record RootItem();

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
