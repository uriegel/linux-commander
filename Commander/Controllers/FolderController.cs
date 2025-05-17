using Commander.DataContexts;
using Commander.UI;
using CsTools.HttpRequest;
using static System.Console;

namespace Commander.Controllers;

class FolderController
{
    public FolderController(FolderView folderView)
    {
        this.folderView = folderView;
        controller = new RootController(folderView);
        Actions.Instance.PropertyChanged += (s, e) =>
        {
            folderView.Context.CurrentDirectories = Actions.Instance.ShowHidden ? controller.Directories + controller.HiddenDirectories : controller.Directories;
            folderView.Context.CurrentFiles = Actions.Instance.ShowHidden ? controller.Files + controller.Files : controller.Files;
        };
    }

    public string CurrentPath { get => controller.CurrentPath; }

    public string? GetItemPath(int pos) => controller.GetItemPath(pos);
    public ExifData? GetExifData(int pos) => controller.GetExifData(pos);

    public Task<bool> DeleteItems() => controller.DeleteItems();
    public Task<bool> Rename() => controller.Rename();

    public Task<bool> ExtendedRename() => controller.ExtendedRename(folderView);

    public Task<bool> CreateFolder() => controller.CreateFolder();
    public Task<bool> CopyItems(string? targetPath, bool move) => controller.CopyItems(targetPath, move);

    public async void OnActivate(int pos)
    {
        try
        {
            var newPath = await controller.OnActivate(pos);
            if (!string.IsNullOrEmpty(newPath))
                ChangePath(newPath, true);
        }
        catch (Exception e)
        {
            Error.WriteLine($"Fehler bei OnActivate: {e}");
        }
    }

    public async void ChangePath(string path, bool saveHistory)
    {
        DetectController(path);
        try
        {
            controller.RemoveAll();
            var lastPos = await controller.Fill(path, folderView);
            if (lastPos != -1)
                folderView.ScrollTo(lastPos);
            folderView.Context.CurrentDirectories = Actions.Instance.ShowHidden ? controller.Directories + controller.HiddenDirectories : controller.Directories;
            folderView.Context.CurrentFiles = Actions.Instance.ShowHidden ? controller.Files + controller.Files : controller.Files;
            folderView.Context.CurrentPath = controller.CurrentPath;
            folderView.OnPathChanged(saveHistory ? CurrentPath : null);
            folderView.InvalidateFocus();
        }
        catch (UnauthorizedAccessException uae)
        {
            OnError(uae);
            MainContext.Instance.ErrorText = "Kein Zugriff";
        }
        catch (DirectoryNotFoundException dnfe)
        {
            OnError(dnfe);
            MainContext.Instance.ErrorText = "Pfad nicht gefunden";
        }
        catch (RequestException re) when (re.CustomRequestError == CustomRequestError.ConnectionError)
        {
            OnError(re);
            MainContext.Instance.ErrorText = "Die Verbindung zum Gerät konnte nicht aufgebaut werden";
        }
        catch (RequestException re) when (re.CustomRequestError == CustomRequestError.NameResolutionError)
        {
            OnError(re);
            MainContext.Instance.ErrorText = "Der Netzwerkname des Gerätes konnte nicht ermittelt werden";
        }
        catch (Exception e)
        {
            OnError(e);
            MainContext.Instance.ErrorText = "Ordner konnte nicht gewechselt werden";
        }

        void OnError(Exception e)
        {
            if (!folderView.Context.CurrentPath.StartsWith("remote/"))
            {
                var refreshPath = folderView.Context.CurrentPath;
                folderView.Context.CurrentPath = "";
                folderView.Context.CurrentPath = refreshPath;
                ChangePath(folderView.Context.CurrentPath, false);
            }
            folderView.GrabFocus();
            Error.WriteLine($"Konnte Pfad nicht ändern: {e}");
        }
    }

    public bool CheckRestriction(string searchKey) => controller.CheckRestriction(searchKey);

    public int GetFocusedItemPos() => controller.GetFocusedItemPos();
    public int ItemsCount() => controller.ItemsCount();

    public int OnSelectionChanged(nint model, int pos, int count, bool mouseButton, bool mouseButtonCtrl)
        => controller.OnSelectionChanged(model, pos, count, mouseButton, mouseButtonCtrl);

    public void SelectAll() => controller.SelectAll(folderView);
    public void SelectNone() => controller.SelectNone(folderView);
    public void SelectCurrent() => controller.SelectCurrent(folderView);
    public void SelectToStart() => controller.SelectToStart(folderView);
    public void SelectToEnd() => controller.SelectToEnd(folderView);

    bool DetectController(string path)
    {
        return path switch
        {
            "fav" => SetController(() => new FavoritesController(folderView)),
            "remotes" => SetController(() => new RemotesController(folderView)),
            "root" => SetController(() => new RootController(folderView)),
            "" => SetController(() => new RootController(folderView)),
            _ when path.StartsWith("remote") => SetController(() => new RemoteController(folderView)),
            _ => SetController(() => new DirectoryController(folderView))
        };

        bool SetController<T>(Func<T> controller)
            where T : IController
        {
            if (this.controller is not T)
            {
                this.controller.Dispose();
                this.controller = controller();
                return true;
            }            
            else
                return false;
        }
    }

    IController controller;
    FolderView folderView;
}

