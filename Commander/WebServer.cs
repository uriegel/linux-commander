using System;
using System.Threading.Tasks;
using UwebServer;
using UwebServer.Routes;

static class WebServer
{
    public static void Start() => server.Start();
    public static void Stop() => server.Stop();

    static WebServer()
    {
        var startTime = DateTime.Now;
        var routeWebSite = FileServing.Create(startTime);
        var routeService = new JsonService("/commander", async input =>
        {
            switch (input.Path)
            {
                case "getitems":
                    var getItems = input.RequestParam.Get<GetItems>();
                    var items = DirectoryProcessor.GetItems(getItems.Path, getItems.HiddenIncluded);
                    return items;
                case "getroot":
                    var rootItems = await RootProcessor.GetItemsAsync();
                    return rootItems;
                default:
                    return new { };
            }
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
        public override async Task ProcessAsync(IRequest request, IRequestHeaders headers, Response response)
        {
            var query = new UrlComponents(headers.Url[11..]);
            switch (query.Path)
            {
                case "geticon":
                    {
                        var ext = query.Parameters["ext"];
                        await FileServing.ServeIconAsync(ext, response);
                        break;
                    }
                default:
                    await response.SendNotFoundAsync();
                    break;
            }
        }
    }
    static readonly Server server;
}

record GetItems(string Id, string Path, bool HiddenIncluded);

