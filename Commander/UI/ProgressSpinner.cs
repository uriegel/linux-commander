using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

public class ProgressSpinner : SubClassWidgetInst<DrawingAreaHandle>
{
    public static SubClass<DrawingAreaHandle> Subclass()
        => new ProgressSpinnerClass("ProgressSpinner", p => new ProgressSpinner(p));

    protected override DrawingAreaHandle CreateHandle(nint obj) => new(obj);

    public ProgressSpinner(nint obj) : base(obj) { }

    protected override void OnInitialize()
    {
        Handle
            .CssClass("custom-accent")
            .SetDrawFunction((_, cairo, w, h) =>
            {
                var color = ProgressContext.Instance.CopyProgress?.IsRunning == true
                    ? Handle.GetStyleContext().GetColor().ToSrgb()
                    : new GtkRgba() { Red = 0, Green = 0, Blue = 0, Alpha = 0 };
                cairo
                    .AntiAlias(CairoAntialias.Best)
                    .LineCap(LineCap.Round)
                    .LineWidth(3.0)
                    .SourceRgba(color.Red, color.Green, color.Blue, 0.2)
                    .Arc(w / 2.0, h / 2.0, (w < h ? w : h) / 2.0 - 2.0, -Math.PI / 2.0, -Math.PI / 2.0 + Math.PI * 2)
                    .Stroke()
                    .AntiAlias(CairoAntialias.Best)
                    .LineCap(LineCap.Round)
                    .LineWidth(3.0)
                    .SourceRgba(color.Red, color.Green, color.Blue, color.Alpha)
                    .Arc(w / 2.0, h / 2.0, (w < h ? w : h) / 2.0 - 2.0, -Math.PI / 2.0, -Math.PI / 2.0 + progress * Math.PI * 2)
                    .Stroke();
            });
        ProgressContext.Instance.PropertyChanged += (s, e) => OnDraw();

        var window = Handle.GetAncestor<AdwApplicationWindowHandle>();
        totalProgress = window.GetTemplateChild<ProgressBarHandle, AdwApplicationWindowHandle>("progress-bar-total");
        currentProgress = window.GetTemplateChild<ProgressBarHandle, AdwApplicationWindowHandle>("progress-bar-current");
    }

    protected override void OnFinalize() => Console.WriteLine("ProgressDisplay finalized");

    void OnDraw()
    {
        var cpc = ProgressContext.Instance.CopyProgress;
        if (cpc != null)
        {
            progress = (cpc.TotalBytes + cpc.CurrentBytes) / (float)cpc.TotalMaxBytes;
            Handle.QueueDraw();
        }
    }

    float progress;
    ProgressBarHandle totalProgress = new(0);
    ProgressBarHandle currentProgress = new(0);
}

public class ProgressSpinnerClass(string name, Func<nint, ProgressSpinner> constructor)
    : SubClass<DrawingAreaHandle>(GTypeEnum.DrawingArea, name, constructor)
{ }
