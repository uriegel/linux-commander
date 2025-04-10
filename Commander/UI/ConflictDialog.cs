using System.Reflection.Metadata;
using Commander.Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;

namespace Commander.UI;

class ConflictDialog
{
    public ConflictDialog(IEnumerable<CopyItem> conflicts)
    {
        var builder = Builder.FromDotNetResource("conflictdialog");
        dialog = builder.GetWidget<AdwDialogHandle>("dialog");
        var columnViewHandle = builder.GetWidget<CustomColumnViewHandle>("conflictview");
        var columnView = ConflictView.GetInstance(columnViewHandle);
        columnView?.SetTabBehavior(ListTabBehavior.Item);
        columnView?.Fill(conflicts);
        var yesButton = builder.GetWidget<ButtonHandle>("yes-button");
        yesButton.OnClicked(() => Console.WriteLine("Ja"));
        var noButton = builder.GetWidget<ButtonHandle>("no-button");
        noButton.OnClicked(() => Console.WriteLine("Nein"));
        dialog.SetDefaultWidget(yesButton);
        var overwriteCritical = conflicts.Any(n => n.Target != null && n.Target.DateTime > n.Source.DateTime);
        yesButton.AddCssClass("destructive-action", overwriteCritical);
        noButton.AddCssClass("suggested-action", overwriteCritical);
        yesButton.AddCssClass("suggested-action", !overwriteCritical);
        noButton.AddCssClass("destructive-action", !overwriteCritical);
    }

    public void Show() => dialog.Present(MainWindow.MainWindowHandle);

    readonly AdwDialogHandle dialog = new(0);
}

class ConflictView : ColumnViewSubClassed
{
    public ConflictView(nint obj)
        : base(obj)
    {
        MultiSelection = true;
        OnSelectionChanged = (model, pos, count) => model.UnselectRange(pos, count);
        controller = new(this);
    }

    protected override void OnCreate()
    {
        OnActivate(_ => Console.WriteLine("aktiviziert"));
    }

    public void Fill(IEnumerable<CopyItem> conflicts) => controller.Fill(conflicts);

    public static ConflictView? GetInstance(CustomColumnViewHandle handle)
        => GetInstance(handle.GetInternalHandle()) as ConflictView;

    protected override void OnFinalize()
        => Console.WriteLine("ConflictView finalized");

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    ConflichtController controller;
}

class ConflictViewClass() : ColumnViewSubClassedClass("ConflictView", p => new ConflictView(p)) { }