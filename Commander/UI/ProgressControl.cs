using Commander.DataContexts;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

public class ProgressControl : SubClassInst<RevealerHandle>
{
    public static SubClass<RevealerHandle> Subclass()
        => new ProgressControlClass("ProgressControl", p => new ProgressControl(p));

    protected override RevealerHandle CreateHandle(nint obj) => new(obj);

    public ProgressControl(nint obj) : base(obj) { }

    protected override void OnCreate()
    {
        var builder = Builder.FromDotNetResource("progresscontrol");
        var button = builder.GetWidget<MenuButtonHandle>("progress-control");
        Handle.Child(button);

        Handle
            .DataContext(CopyProgressContext.Instance)
            .Binding("reveal-child", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, p => p != null);
        builder.GetWidget<MenuButtonHandle>("title-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => ((CopyProgress?)cpc)?.Title);
        builder.GetWidget<MenuButtonHandle>("size-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"({((CopyProgress?)cpc)?.TotalMaxBytes.ByteCountToString(2)})");
        builder.GetWidget<MenuButtonHandle>("current-name-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => ((CopyProgress?)cpc)?.Name);
        builder.GetWidget<MenuButtonHandle>("progressbar-total")
            .Binding("fraction", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, CopyProgressContext.GetTotalFraction);
        builder.GetWidget<MenuButtonHandle>("progressbar-current")
            .Binding("fraction", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, CopyProgressContext.GetFraction);
        builder.GetWidget<MenuButtonHandle>("total-count-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.TotalCount}");
        builder.GetWidget<MenuButtonHandle>("current-count-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.CurrentCount}");
        builder.GetWidget<MenuButtonHandle>("duration-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{((CopyProgress?)cpc)?.Duration:hh\\:mm\\:ss}");
        builder.GetWidget<MenuButtonHandle>("estimated-duration-label")
            .Binding("label", nameof(CopyProgressContext.CopyProgress), BindingFlags.Default, cpc => $"{CopyProgressContext.GetEstimatedDuration(cpc):hh\\:mm\\:ss}");
        builder.GetWidget<MenuButtonHandle>("cancel-btn")
            .OnClicked(CopyProgressContext.Cancel);
    }
}

public class ProgressControlClass(string name, Func<nint, ProgressControl> constructor)
    : SubClass<RevealerHandle>(GTypeEnum.Revealer, name, constructor)
{ }
