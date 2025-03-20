using Commander.UI;

namespace Commander.Controllers;

interface IController : IDisposable
{
    string CurrentPath { get; }
    Task<int> Fill(string path);

    string? OnActivate(uint pos);

    void IDisposable.Dispose() {}
}
