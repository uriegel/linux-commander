using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

public static class AddRemote
{
    public static async Task<string?> ShowAsync()
    {
        var builder = Builder.FromDotNetResource("addremote");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        // var textView = builder.GetWidget<EntryHandle>("text");
        // textView.Text(preset ?? "");
        dialog.OnMap(async () =>
        {
//            textView.GrabFocus();
            await Task.Delay(100);
        });

        return await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok"
            ? "textView.GetText()"
            : null;
    }
}