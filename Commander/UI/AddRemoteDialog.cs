using Commander.Controllers;
using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

static class AddRemoteDialog
{
    public static async Task<RemotesItem?> ShowAsync()
    {
        var builder = Builder.FromDotNetResource("addremote");
        var dialog = builder.GetWidget<AdwAlertDialogHandle>("dialog");
        var textView = builder.GetWidget<EntryHandle>("text");
        var ipView = builder.GetWidget<EntryHandle>("ip");
        var isAndroid = builder.GetWidget<CheckButtonHandle>("android");
        dialog.OnMap(textView.GrabFocus);

        return await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok"
            ? new(textView.GetText() ?? "", ipView.GetText() ?? "", isAndroid?.IsActive() == true)
            : null;
    }
}