using Commander.UI;

namespace Commander.Controllers;

interface IController
{
    void Fill(string path);

    string? OnActivate(uint pos);
}
