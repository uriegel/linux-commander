using GtkDotNet;
using GtkDotNet.SafeHandles;
using static GtkDotNet.Controls.ColumnViewSubClassed;

abstract class ControllerBase<T> : Controller<T>
    where T: class
{
    public int GetFocusedItemPos(WindowHandle window)
    {
        // TODO g_type_name
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
            var idx = 0;
            foreach (var item in Items())
            {
                if (item == focusedItem)
                    break;
                idx++;
            }
            return idx;
        }
        else
            return -1;
    }
}