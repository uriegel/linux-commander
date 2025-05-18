using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using static GtkDotNet.Controls.ColumnViewSubClassed;

namespace Commander.Controllers;

abstract class ControllerBase<T> : Controller<T>
    where T : class
{
    public ControllerBase() {}
    public ControllerBase(SelectionHandle selectionModel) => this.selectionModel = selectionModel;

    protected IEnumerable<int> GetSelectedItemsIndexes()
    {
        if (selectionModel == null)
            yield break;
        var count = ItemsCount();
        var idx = 0;
        while (true)
        {
            if (selectionModel.IsSelected(idx))
                yield return idx;
            idx++;
            if (idx == count)
                yield break;
        }
    }

    protected IEnumerable<T> GetSelectedItems(int? focusedItemPos = null)
    {
        var items = selectionModel != null
            ? GetSelectedItemsIndexes()
                .SelectFilterNull(selectionModel.GetItem<T>)
            : [];
        if (items.Any())
            return items;
        if (focusedItemPos.HasValue)
        {
            var focusedItem = GetItem(focusedItemPos.Value);
            return focusedItem != null
                ? [focusedItem]
                : [];
        }
        else
            return [];
    }

    protected readonly SelectionHandle? selectionModel;
}
