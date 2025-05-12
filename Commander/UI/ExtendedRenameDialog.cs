using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

public static class ExtendedRenameDialog
{
    public static async Task<string?> ShowAsync()
    {
        var builder = Builder.FromDotNetResource("extendedrenamedialog");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        var textView = builder.GetWidget<EntryHandle>("text");
        // dialog.Heading(heading);
        // dialog.Body(text);
        // textView.Text(preset ?? "");
        dialog.OnMap(async () =>
        {
            textView.GrabFocus();
            await Task.Delay(100);
            // if (onSelect != null)
            // {
            //     var (start, end) = onSelect();
            //     textView.SelectRegion(start, end);
            // }
        });

        return await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok"
            ? textView.GetText()
            : null;
    }
}