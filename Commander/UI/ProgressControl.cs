using Commander.DataContexts;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

public class ProgressControl : SubClassInst<RevealerHandle>
{
    public static SubClass<RevealerHandle> Subclass()
        => new ProgressControlClass("ProgressControl", p => new ProgressControl(p));

    public static ProgressControl? GetInstance(RevealerHandle? handle)
        => (handle != null ? GetInstance(handle.GetInternalHandle()) : null) as ProgressControl;

    public void ShowPopover()
    {
        if (CopyProgressContext.Instance.CopyProgress != null)
            menuButton.Popup();
    } 

    protected override RevealerHandle CreateHandle(nint obj) => new(obj);

    public ProgressControl(nint obj) : base(obj) { }

    protected override void OnCreate()
    {
        var builder = Builder.FromDotNetResource("progresscontrol");
        menuButton = builder.GetWidget<MenuButtonHandle>("progress-control");
        Handle.Child(menuButton);

        Handle
            .DataContext(CopyProgressContext.Instance)
            .Binding("reveal-child", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, p => p != null);
        builder.GetWidget<LabelHandle>("title-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => ((CopyProgress?)cpc)?.Title);
        builder.GetWidget<LabelHandle>("size-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"({((CopyProgress?)cpc)?.TotalMaxBytes.ByteCountToString(2)})");
        builder.GetWidget<LabelHandle>("current-name-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => ((CopyProgress?)cpc)?.Name);
        builder.GetWidget<ProgressBarHandle>("progressbar-total")
            .Binding("fraction", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, CopyProgressContext.GetTotalFraction);
        builder.GetWidget<ProgressBarHandle>("progressbar-current")
            .Binding("fraction", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, CopyProgressContext.GetFraction);
        builder.GetWidget<LabelHandle>("total-count-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.TotalCount}");
        builder.GetWidget<LabelHandle>("current-count-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.CurrentCount}");
        builder.GetWidget<LabelHandle>("duration-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.Duration:hh\\:mm\\:ss}");
        builder.GetWidget<LabelHandle>("estimated-duration-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{CopyProgressContext.GetEstimatedDuration(cpc):hh\\:mm\\:ss}");
        builder.GetWidget<ButtonHandle>("cancel-btn")
            .OnClicked(CopyProgressContext.Cancel);
    }

    MenuButtonHandle menuButton = new(0);
}

public class ProgressControlClass(string name, Func<nint, ProgressControl> constructor)
    : SubClass<RevealerHandle>(GTypeEnum.Revealer, name, constructor)
{ }
