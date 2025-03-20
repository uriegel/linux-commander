using Commander.UI;

namespace Commander.Controllers;

interface IController : IDisposable
{
    void Fill(string path);

    string? OnActivate(uint pos);

    void IDisposable.Dispose() {}
}
