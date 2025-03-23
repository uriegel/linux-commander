using Commander.UI;
using GtkDotNet.SafeHandles;

namespace Commander.Controllers;

interface IController : IDisposable
{
    string CurrentPath { get; }
    Task<int> Fill(string path);

    string? OnActivate(int pos);

    int GetFocusedItemPos();
    int ItemsCount();

    void OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl);

    void SelectAll(FolderView folderView);
    void SelectNone(FolderView folderView);
    void SelectCurrent(FolderView folderView, WindowHandle window);
    void SelectToStart(FolderView folderView, WindowHandle window);
    void SelectToEnd(FolderView folderView, WindowHandle window);

    void IDisposable.Dispose() { }
}
