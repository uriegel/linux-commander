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
        var digits = builder.GetWidget<WidgetHandle>("digits");
        var start = builder.GetWidget<WidgetHandle>("start");
 
        var data = Storage.Retrieve().ExtendedRenameData ?? new("Bild", 2, 1);
        prefixText.Text(data.Prefix);
        digits.GetInternalHandle().SetSelected(data.Digits - 1);
        start.GetInternalHandle().SetValue(data.StartIndex);

        dialog.OnMap(async () =>
        {
            start.GrabFocus();
            await Task.Delay(100);
        });

        if (await dialog.PresentAsync(MainWindow.MainWindowHandle) == "ok")
        {
            var prefix = prefixText.GetText() ?? data.Prefix;
            var d = digits.GetInternalHandle().GetSelected() + 1;
            var startVal = start.GetInternalHandle().GetValue();
            data = data with { Prefix = prefix, Digits = d, StartIndex = (int)startVal };
            Storage.SaveExtendedRename(data);
            
        }
        return "";
    }

    [System.Runtime.InteropServices.DllImport("libgtk-4.so.1", EntryPoint = "gtk_drop_down_get_selected", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    extern static int GetSelected(this nint misdt);
    [System.Runtime.InteropServices.DllImport("libgtk-4.so.1", EntryPoint = "gtk_drop_down_set_selected", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    extern static void SetSelected(this nint misdt, int pos);
    [System.Runtime.InteropServices.DllImport("libgtk-4.so.1", EntryPoint = "gtk_spin_button_get_value", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    extern static double GetValue(this nint misdt);
    [System.Runtime.InteropServices.DllImport("libgtk-4.so.1", EntryPoint = "gtk_spin_button_set_value", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    extern static void SetValue(this nint misdt, double value);
}

record ExtendedRenameData(
    string Prefix,
    int Digits,
    int StartIndex
);