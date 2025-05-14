using Commander.Settings;
using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

static class ExtendedRenameDialog
{
    public static async Task<ExtendedRenameData?> ShowAsync()
    {
        var builder = Builder.FromDotNetResource("extendedrenamedialog");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        var prefixText = builder.GetWidget<EntryHandle>("prefix");
        var digits = builder.GetWidget<DropDownHandle>("digits");
        var start = builder.GetWidget<SpinButtonHandle>("start");

        var data = Storage.Retrieve().ExtendedRenameData ?? new("Bild", 2, 1);
        prefixText.Text(data.Prefix);
        digits.SetSelected(data.Digits - 1);
        start.SetValue(data.StartIndex);

        dialog.OnMap(async () =>
        {
            start.GrabFocus();
            await Task.Delay(100);
        });

        if (await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok")
        {
            var prefix = prefixText.GetText() ?? data.Prefix;
            var d = digits.GetSelected() + 1;
            var startVal = start.GetValue();
            data = data with { Prefix = prefix, Digits = d, StartIndex = (int)startVal };
            Storage.SaveExtendedRename(data);
            return new(prefix, d, (int)startVal);
        }
        else
            return null;
    }
}

record ExtendedRenameData(
    string Prefix,
    int Digits,
    int StartIndex
);