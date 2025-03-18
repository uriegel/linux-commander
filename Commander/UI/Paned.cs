using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;
static class Paned
{
    public static void OnTab(PanedHandle paned, KeyModifiers m)
    {
        // TODO select other ColumnView
        Console.WriteLine("Habe den Täb");
    }
}