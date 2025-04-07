using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

public class ConflictDialog
{
    public ConflictDialog()
    {
        var builder = Builder.FromDotNetResource("conflictdialog");
        dialog = builder.GetWidget<AdwDialogHandle>("dialog");
        var columnViewHandle = builder.GetWidget<CustomColumnViewHandle>("columnview");
        var columnView = FolderView.GetInstance(columnViewHandle);
        columnView?.ChangePath("root");
    }

    public void Show() => dialog.Present(MainWindow.MainWindowHandle);

    AdwDialogHandle dialog = new(0);
}

