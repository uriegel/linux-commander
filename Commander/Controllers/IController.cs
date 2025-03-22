using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.Controllers;

interface IController : IDisposable
{
    string CurrentPath { get; }
    Task<int> Fill(string path);

    string? OnActivate(uint pos);

    int GetFocusedItemPos(WindowHandle window);

    int ItemsCount();

    void OnSelectionChanged(nint model, uint pos, uint count);

    void IDisposable.Dispose() { }

    static void AttachListItem(ListItemHandle listItem)
    {
        var widget = listItem.GetChild<WidgetHandle>();
        var row = widget?.GetParent().GetParent();
        if (row != null && row.GetName() == "GtkColumnViewRowWidget")
            row.SetData("listitem", listItem.GetInternalHandle());
    }   
}
