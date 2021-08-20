#if Linux

using System;
using System.Threading.Tasks;
using GtkDotNet;
using UwebServer;
using UwebServer.Routes;

class FileServing
{
    public static Route Create(DateTime startTime)
        => new WebSite(file => new ResourceStream($"/de/uriegel/commander/web/{file}"), _ => startTime);

    public static async Task ServeIconAsync(string ext, Response response)
    {
        using var iconInfo = IconInfo.Choose(ext, 16, IconLookup.ForceSvg);
        var file = iconInfo.GetFileName();
        await response.SendFileAsync(file);
    }
}

#endif

