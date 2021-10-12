using System;
using System.Linq;
using System.Threading.Tasks;
using UwebServer;
using UwebServer.Routes;

class WebServer
{
    public event EventHandler<RefreshEventArgs> OnRefresh;
    public event EventHandler<ExceptionEventArgs> OnException;
    public void Start() => server.Start();
    public void Stop() => server.Stop();

    public WebServer(ProcessingQueue processingQueue)
    {
        var startTime = DateTime.Now;

        var routeWebSite = FileServing.Create(startTime);
        var routeService = new JsonService("/commander", async input =>
        {
            try
            {
                switch (input.Path)
                {
                    case "getitems":
                        var getItems = input.RequestParam.Get<GetItems>();
                        var items = DirectoryProcessor.GetItems(getItems.Path, getItems.HiddenIncluded, getItems.Id);
                        return items;
                    case "getroot":
                        return await RootProcessor.GetItemsAsync();
                    case "getexifs":
                        var getExifs = input.RequestParam.Get<GetExifs>();
                        return getExifs.ExifItems
                            .Select(n => DirectoryProcessor.GetExifData(getExifs.path, n))
                            .Where(n => n != null);
                    case "copy":
                        DirectoryProcessor.CopyFiles(processingQueue, input.RequestParam.Get<FileItems>());
                        break;
                    case "move":
                        DirectoryProcessor.MoveFiles(processingQueue, input.RequestParam.Get<FileItems>());
                        break;
                    case "delete":
                        DirectoryProcessor.DeleteFiles(processingQueue, input.RequestParam.Get<DeleteItems>());
                        break;
                    case "rename":
                        var renameItem = input.RequestParam.Get<RenameItem>();
                        DirectoryProcessor.RenameFile(renameItem);
                        OnRefresh?.Invoke(this, new(renameItem.Id));
                        break;
                    case "createFolder":
                        var createFolderItem = input.RequestParam.Get<CreateFolder>();
                        DirectoryProcessor.CreateFolder(createFolderItem);
                        OnRefresh?.Invoke(this, new(createFolderItem.Id));
                        break;
                    default:
                        break;
                }
            }
            catch (WebViewException wve)
            {
                OnException?.Invoke(this, new(wve.Message, wve.IDs));
            }
            return new { };
        });
        var routeFile = new FileRequest();

        server = new(new()
        {
            Port = 9865,
            Routes = new Route[]
            {
                routeService,
                routeFile,
                routeWebSite
            }
        });
    }

    class FileRequest : Route
    {
        public FileRequest() : base()
        {
            Method = Method.GET;
            Path = "/commander";
        }
        public override async Task<bool> ProcessAsync(IRequest request, IRequestHeaders headers, Response response)
        {
            var query = new UrlComponents(headers.Url, Path);
            switch (query.Path)
            {
                case "geticon":
                    {
                        var ext = query.Parameters["ext"];
                        await FileServing.ServeIconAsync(ext, response);
                        break;
                    }
                case "getfile":
                    {
                        var file = query.Parameters["file"];
                        await response.SendFileAsync(file);
                        break;
                    }
                default:
                    await response.SendNotFoundAsync();
                    break;
            }
            return true;
        }
    }
    readonly Server server;
}

