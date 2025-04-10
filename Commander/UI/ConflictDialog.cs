using Commander.Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

// TODO 1. Gtk4DotNet All from the above
// TODO 1. Gtk4DotNet AdwDialog Object from Template with PresentAsync

class ConflictDialog(nint obj) : SubClassInst<AdwDialogHandle>(obj)
{
    /// <summary>
    /// Presents the conflict dialog
    /// </summary>
    /// <param name="conflicts"></param>
    /// <returns>true: overwrite conflicts, false: don't overwerite conflicts</returns>
    // TODO zusammenfassen
    public static Task<bool> PresentAsync(IEnumerable<CopyItem> conflicts)
    {
        var dialogHandle = GObject.New<AdwDialogHandle>("ConflictDialog".TypeFromName());
        dialogHandle.Present(MainWindow.MainWindowHandle);
        var dialog = GetInstance(dialogHandle);
        dialog!.conflicts = conflicts;
        return dialog!.completionSource.Task;
    }

    protected override async void OnCreate()
    {
        // TODO zusammenfassen
        Handle.InitTemplate();
        await Task.Delay(1);
        var columnViewHandle = Handle.GetTemplateChild<CustomColumnViewHandle, AdwDialogHandle>("conflictview")!;
        var columnView = ConflictView.GetInstance(columnViewHandle);
        columnView.SetTabBehavior(ListTabBehavior.Item);
        columnView.Fill(conflicts);
        var yesButton = Handle.GetTemplateChild<ButtonHandle, AdwDialogHandle>("yes-button");
        yesButton.OnClicked(() =>
        {
            Handle.CloseDialog();
            completionSource.TrySetResult(true);
        });
        var noButton = Handle.GetTemplateChild<ButtonHandle, AdwDialogHandle>("no-button");
        noButton.OnClicked(() =>
        {
            Handle.CloseDialog();
            completionSource.TrySetResult(false);
        });
        Handle.SetDefaultWidget(yesButton);
        overwriteCritical = conflicts.Any(n => n.Target != null && n.Target.DateTime > n.Source.DateTime);
        yesButton.AddCssClass("destructive-action", overwriteCritical);
        noButton.AddCssClass("suggested-action", overwriteCritical);
        yesButton.AddCssClass("suggested-action", !overwriteCritical);
        noButton.AddCssClass("destructive-action", !overwriteCritical);
        Handle.OnClosed(() => completionSource.TrySetException(new TaskCanceledException()));
    }

    protected override AdwDialogHandle CreateHandle(nint obj) => new(obj);

    protected override void OnFinalize() => Console.WriteLine("ConflictDialog finalized");

    static ConflictDialog? GetInstance(AdwDialogHandle? handle)
        => (handle != null ? GetInstance(handle.GetInternalHandle()) : null) as ConflictDialog;

    IEnumerable<CopyItem> conflicts = [];
    readonly TaskCompletionSource<bool> completionSource = new();

    bool overwriteCritical;
}

class ConflictDialogClass()
    : SubClass<AdwDialogHandle>(GTypeEnum.AdwDialog, "ConflictDialog", p => new ConflictDialog(p))
{
    // TODO to base class, extended constructor with template
    protected override void ClassInit(nint cls, nint _)
    {
        base.ClassInit(cls, _);
        InitTemplateFromResource(cls, "conflictdialog");
    }  
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

    public static ConflictView GetInstance(CustomColumnViewHandle handle)
        => (GetInstance(handle.GetInternalHandle()) as ConflictView)!;

    protected override void OnFinalize()
        => Console.WriteLine("ConflictView finalized");

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    ConflichtController controller;
}

class ConflictViewClass() : ColumnViewSubClassedClass("ConflictView", p => new ConflictView(p)) { }