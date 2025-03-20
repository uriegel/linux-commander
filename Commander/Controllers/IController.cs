using Commander.UI;

namespace Commander.Controllers;

interface IController : IDisposable
{
    string CurrentPath { get; }
    void Fill(string path);

    string? OnActivate(uint pos);

    void IDisposable.Dispose() {}
}
