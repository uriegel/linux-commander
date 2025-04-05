using Commander.DataContexts;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

public class ProgressSpinnerClass(string name, Func<nint, ProgressSpinner> constructor)
    : SubClass<DrawingAreaHandle>(GTypeEnum.DrawingArea, name, constructor)
{ }

public class ProgressSpinner : SubClassInst<DrawingAreaHandle>
{
    public static SubClass<DrawingAreaHandle> Subclass()
        => new ProgressSpinnerClass("ProgressSpinner", p => new ProgressSpinner(p));

    protected override DrawingAreaHandle CreateHandle(nint obj) => new(obj);

    public ProgressSpinner(nint obj) : base(obj) { }

    protected override void OnCreate()
    {
        Handle
            .CssClass("custom-accent")
            .SetDrawFunction((_, cairo, w, h) =>
            {
                var color = Handle.GetStyleContext().GetColor().ToSrgb();
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
        CopyProgressContext.Instance.PropertyChanged += (s, e) => OnDraw();
    }

    protected override void OnFinalize()
    {
        Console.WriteLine("ProgressDisplay finalized");
    }

    void OnDraw()
    {
        var cpc = CopyProgressContext.Instance.CopyProgress;
        if (cpc != null)
        {
            progress = (cpc.TotalBytes + cpc.CurrentBytes) / (float)cpc.TotalMaxBytes;
            Gtk.Dispatch(() => Handle.QueueDraw());
        }
    }

    float progress;
}