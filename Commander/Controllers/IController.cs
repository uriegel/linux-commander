using Commander.UI;

namespace Commander.Controllers;

interface IController
{
    void Fill();

    string? OnActivate(uint pos);
}
