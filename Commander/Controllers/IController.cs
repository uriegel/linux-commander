using Commander.UI;

namespace Commander.Controllers;

interface IController : IDisposable
{
    string CurrentPath { get; }
    int Directories { get; }
    int Files { get; }

    string? GetItemPath(int pos);

    ExifData? GetExifData(int pos);

    Task<int> Fill(string path, FolderView folderView);

    string? OnActivate(int pos);

    int GetFocusedItemPos();
    int ItemsCount();

    bool CheckRestriction(string searchKey);

    void OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl);

    void SelectAll(FolderView folderView);
    void SelectNone(FolderView folderView);
    void SelectCurrent(FolderView folderView);
    void SelectToStart(FolderView folderView);
    void SelectToEnd(FolderView folderView);

    void IDisposable.Dispose() { }
}
