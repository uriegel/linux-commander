using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

public static class TextDialog
{
    public static async Task<string?> ShowAsync(string heading, string text, string? preset = null, Func<(int, int)>? onSelect = null)
    {
        var builder = Builder.FromDotNetResource("textdialog");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        var textView = builder.GetWidget<EntryHandle>("text");
        dialog.Heading(heading);
        dialog.Body(text);
        textView.Text(preset ?? "");
        dialog.OnMap(async () =>
        {
            textView.GrabFocus();
            await Task.Delay(100);
            if (onSelect != null)
            {
                var (start, end) = onSelect();
                textView.SelectRegion(start, end);
            }
        });

        return await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok"
            ? textView.GetText()
            : null;
    }
}