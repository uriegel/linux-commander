using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

class AlertDialog(nint obj) : GtkDotNet.SubClassing.AlertDialog(obj)
{
    public static Task<string> PresentAsync(string heading, string text)
    {
        var handle = GObject.New<AdwAlertDialogHandle>("AlertDialog".TypeFromName());
        handle.Heading(heading);
        handle.Body(text);
        return handle.PresentAsync(MainWindow.MainWindowHandle);
    }

    protected override void OnFinalize() => Console.WriteLine("AlertDialog finalized");
}

class AlertDialogClass() 
    : GtkDotNet.SubClassing.AlertDialogClass("AlertDialog", "alertdialog", p => new AlertDialog(p))
{ }

