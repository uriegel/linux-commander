#if Windows
using System;
using System.Threading.Tasks;

using UwebServer;
using UwebServer.Routes;

class FileServing
{
    public static Route Create(DateTime startTime)
        => new WebSite(file => null, _ => startTime);

    public static async Task ServeIconAsync(string ext, Response response)
    {
    }
}

#endif