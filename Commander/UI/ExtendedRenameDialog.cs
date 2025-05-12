using Commander.Settings;
using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

public static class ExtendedRenameDialog
{
    public static async Task<string?> ShowAsync()
    {
        var builder = Builder.FromDotNetResource("extendedrenamedialog");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        var prefixText = builder.GetWidget<EntryHandle>("prefix");
        var start = builder.GetWidget<WidgetHandle>("start");
        // dialog.Heading(heading);
        // dialog.Body(text);

        var data = Storage.Retrieve().ExtendedRenameData ?? new("Bild", 3, 1);

        prefixText.Text(data.Prefix);
        dialog.OnMap(async () =>
        {
            start.GrabFocus();
            await Task.Delay(100);
            // if (onSelect != null)
            // {
            //     var (start, end) = onSelect();
            //     textView.SelectRegion(start, end);
            // }
        });

        if (await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok")
        {
            var prefix = prefixText.GetText() ?? data.Prefix;
            data = data with { Prefix = prefix };
            Storage.SaveExtendedRename(data);
        }
        return "";
    }
}

record ExtendedRenameData(
    string Prefix,
    int Digits,
    int StartIndex
);