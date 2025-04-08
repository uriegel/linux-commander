using Commander.Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

public class ConflictDialog
{
    public ConflictDialog()
    {
        var builder = Builder.FromDotNetResource("conflictdialog");
        dialog = builder.GetWidget<AdwDialogHandle>("dialog");
        var columnViewHandle = builder.GetWidget<CustomColumnViewHandle>("conflictview");
        var columnView = ConflictView.GetInstance(columnViewHandle);
        columnView?.SetTabBehavior(ListTabBehavior.Item);
    }

    public void Show() => dialog.Present(MainWindow.MainWindowHandle);

    readonly AdwDialogHandle dialog = new(0);
}

class ConflictView : ColumnViewSubClassed
{
    public ConflictView(nint obj)
        : base(obj)
    {
        controller = new(this);
    }

    public static ConflictView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as ConflictView;

    protected override void OnFinalize()
        => Console.WriteLine("ConflictView finalized");

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    ConflichtController controller;
}

class ConflictViewClass() : ColumnViewSubClassedClass("ConflictView", p => new ConflictView(p)) { }