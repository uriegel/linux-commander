using WebServerLight;

namespace Commander;

static class Requests
{
    public static async Task<bool> Process(IRequest request)
    {
        return request.SubPath switch
        {
            "init" => await Init(request),
            _ => false
        };
    }

    static async Task<bool> Init(IRequest request)
    {
        // TODO send saved changePath (here or in browser) with columns)
        //var data = await request.DeserializeAsync<Data>();
        var response = new Response([
            new Contact("Charlie Parker", 34),
            new Contact("Miles Davis", 90),
            new Contact("John Coltrane", 99)], 123, request.Url);

        await request.SendJsonAsync(response);
        return true;
    }
}
record Response(IEnumerable<Contact> Contacts, int ID, string Name);
record Contact(string Name, int Id);
