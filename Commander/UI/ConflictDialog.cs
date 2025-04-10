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
        yesButton.OnClicked(async () =>
        {
            AdwDialog.Close(dialog);
            await Task.Delay(1);
            completionSource.TrySetResult(true);
        });
        var noButton = builder.GetWidget<ButtonHandle>("no-button");
        noButton.OnClicked(async () =>
        {
            AdwDialog.Close(dialog);
            await Task.Delay(1);
            completionSource.TrySetResult(false);
        });
        dialog.SetDefaultWidget(yesButton);
        overwriteCritical = conflicts.Any(n => n.Target != null && n.Target.DateTime > n.Source.DateTime);
        yesButton.AddCssClass("destructive-action", overwriteCritical);
        noButton.AddCssClass("suggested-action", overwriteCritical);
        yesButton.AddCssClass("suggested-action", !overwriteCritical);
        noButton.AddCssClass("destructive-action", !overwriteCritical);
        dialog.OnClosed(() => completionSource.TrySetException(new TaskCanceledException()));
    }

    /// <summary>
    /// Presents the conflict dialog
    /// </summary>
    /// <returns>true: overwrite conflicts, false: don't overwerite conflicts</returns>
    public Task<bool> ShowAsync()
    {
        dialog.Present(MainWindow.MainWindowHandle);
        return completionSource.Task;
    }

    readonly TaskCompletionSource<bool> completionSource = new();
    readonly AdwDialogHandle dialog = new(0);

    bool overwriteCritical;
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
        OnActivate(_ =>
        {
            //overwriteCritical
        });
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