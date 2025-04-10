using Commander.Controllers;
using GtkDotNet;
using GtkDotNet.Controls;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class ConflictDialog(nint obj) : Dialog<bool>(obj)
{
    /// <summary>
    /// Presents the conflict dialog
    /// </summary>
    /// <param name="conflicts"></param>
    /// <returns>true: overwrite conflicts, false: don't overwerite conflicts</returns>
    public static Task<bool> PresentAsync(IEnumerable<CopyItem> conflicts)
    {
        var handle = GObject.New<AdwDialogHandle>("ConflictDialog".TypeFromName());
        var dialog = (GetInstance(handle.GetInternalHandle()) as ConflictDialog)!;
        dialog.Initialize(conflicts);
        return dialog.PresentAsync(MainWindow.MainWindowHandle);
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        var columnViewHandle = Handle.GetTemplateChild<CustomColumnViewHandle, AdwDialogHandle>("conflictview");
        var columnView = ConflictView.GetInstance(columnViewHandle);
        columnView.SetTabBehavior(ListTabBehavior.Item);
        columnView.Fill(conflicts);
        var yesButton = Handle.GetTemplateChild<ButtonHandle, AdwDialogHandle>("yes-button");
        yesButton.OnClicked(() => Close(true));
        var noButton = Handle.GetTemplateChild<ButtonHandle, AdwDialogHandle>("no-button");
        noButton.OnClicked(() => Close(false));
        Handle.SetDefaultWidget(yesButton);
        var overwriteCritical = conflicts.Any(n => n.Target != null && n.Target.DateTime > n.Source.DateTime);
        columnView.OnEnter += (s, e) => Close(overwriteCritical);
        yesButton.AddCssClass("destructive-action", overwriteCritical);
        noButton.AddCssClass("suggested-action", overwriteCritical);
        yesButton.AddCssClass("suggested-action", !overwriteCritical);
        noButton.AddCssClass("destructive-action", !overwriteCritical);
    }

    protected override void OnFinalize() => Console.WriteLine("ConflictDialog finalized");

    void Initialize(IEnumerable<CopyItem> conflicts) => this.conflicts = conflicts;

    IEnumerable<CopyItem> conflicts = [];
}

class ConflictDialogClass() 
    : DialogClass<bool>("ConflictDialog", "conflictdialog", p => new ConflictDialog(p))
{ }

class ConflictView : ColumnViewSubClassed
{
    public ConflictView(nint obj)
        : base(obj)
    {
        MultiSelection = true;
        OnSelectionChanged = (model, pos, count) => model.UnselectRange(pos, count);
        controller = new(this);
    }

    public event EventHandler? OnEnter;

    protected override void OnCreate()
        => OnActivate(_ => OnEnter?.Invoke(this, EventArgs.Empty));

    public void Fill(IEnumerable<CopyItem> conflicts) => controller.Fill(conflicts);

    public static ConflictView GetInstance(CustomColumnViewHandle handle)
        => (GetInstance(handle.GetInternalHandle()) as ConflictView)!;

    protected override void OnFinalize()
        => Console.WriteLine("ConflictView finalized");

    protected override CustomColumnViewHandle CreateHandle(nint obj) => new(obj);

    ConflichtController controller;
}

class ConflictViewClass() : ColumnViewSubClassedClass("ConflictView", p => new ConflictView(p)) { }