using GtkDotNet;
using GtkDotNet.SafeHandles;
using static GtkDotNet.Controls.ColumnViewSubClassed;

abstract class ControllerBase<T> : Controller<T>
    where T : class
{
    public int GetFocusedItemPos(WindowHandle window)
    {
        var row = window.GetFocus<WidgetHandle>();
        if (!row.IsInvalid && row.GetName() == "GtkColumnViewRowWidget")
        {
            ListItemHandle listItem = new(row.GetData("listitem"));
            // TODO so
            // var model = columnView.GetModel<SelectionHandle>();
            // var items = model.GetRawItems();
            // return (uint)items.TakeWhile(n => n != item).Count();
            // TODO check max count and -1!

            var focusedItem = listItem.GetObject<T>();

            return Items().TakeWhile(n => n != focusedItem).Count();
        }
        else
            return -1;
    }

    public int ItemsCount() => RawItems().Count();
}
