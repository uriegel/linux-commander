using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

public class ProgressControl : SubClassInst<RevealerHandle>
{
    public static SubClass<RevealerHandle> Subclass()
        => new ProgressControlClass("ProgressControl", p => new ProgressControl(p));

    public static ProgressControl GetInstance(RevealerHandle handle)
        => (GetInstance(handle.GetInternalHandle()) as ProgressControl)!;

    public void ShowPopover()
    {
        if (ProgressContext.Instance.CopyProgress != null)
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
            .DataContext(ProgressContext.Instance)
            .Binding("reveal-child", nameof(ProgressContext.CopyProgress), BindingFlags.Default, p => p != null);
        builder.GetWidget<LabelHandle>("title-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => ((CopyProgress?)cpc)?.Title);
        builder.GetWidget<LabelHandle>("size-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => $"({((CopyProgress?)cpc)?.TotalMaxBytes.ByteCountToString(2)})")
            .Binding("opacity", nameof(ProgressContext.DeleteAction), BindingFlags.Default, hide => (bool?)hide == true ? 0.0 : 1.0);
        builder.GetWidget<LabelHandle>("current-name-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => ((CopyProgress?)cpc)?.Name);
        builder.GetWidget<ProgressBarHandle>("progressbar-total")
            .Binding("fraction", nameof(ProgressContext.CopyProgress), BindingFlags.Default, ProgressContext.GetTotalFraction);
        builder.GetWidget<ProgressBarHandle>("progressbar-current")
            .Binding("fraction", nameof(ProgressContext.CopyProgress), BindingFlags.Default, ProgressContext.GetFraction);
        builder.GetWidget<LabelHandle>("total-count-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.TotalCount}");
        builder.GetWidget<LabelHandle>("current-count-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.CurrentCount}");
        builder.GetWidget<LabelHandle>("duration-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.Duration:hh\\:mm\\:ss}");
        builder.GetWidget<LabelHandle>("estimated-duration-label")
            .Binding("label", nameof(ProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{ProgressContext.GetEstimatedDuration(cpc):hh\\:mm\\:ss}");
        builder.GetWidget<ButtonHandle>("cancel-btn")
            .OnClicked(ProgressContext.Cancel);
    }

    MenuButtonHandle menuButton = new(0);
}

public class ProgressControlClass(string name, Func<nint, ProgressControl> constructor)
    : SubClass<RevealerHandle>(GTypeEnum.Revealer, name, constructor)
{ }
