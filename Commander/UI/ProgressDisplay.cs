using Commander.DataContexts;
using GtkDotNet;
using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class ProgressDisplay(nint obj) : SubClassInst<DrawingAreaHandle>(obj)
{
    protected override void OnCreate()
    {
        Handle.SetDrawFunction((_, cairo, w, h) =>
            cairo
                .AntiAlias(CairoAntialias.Best)
                .LineJoin(LineJoin.Miter)
                .LineCap(LineCap.Round)
                .Translate(w / 2.0, h / 2.0)
                .StrokePreserve()
                .ArcNegative(0, 0, (w < h ? w : h) / 2.0, -Math.PI / 2.0, -Math.PI / 2.0 + progress * Math.PI * 2)
                .LineTo(0, 0)
                .SourceRgb(0.7, 0.7, 0.7)
                .Fill()
                .MoveTo(0, 0)
                .Arc(0, 0, (w < h ? w : h) / 2.0, -Math.PI / 2.0, -Math.PI / 2.0 + progress * Math.PI * 2)
                .SourceRgb(0.3, 0.3, 0.3)
                .Fill());
        CopyProgressContext.Instance.PropertyChanged += (s, e) => OnDraw();
    }

    protected override DrawingAreaHandle CreateHandle(nint obj) => new(obj);

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
            Console.WriteLine($"Wert: {cpc.CurrentBytes}, {progress}");
            //Gtk.Dispatch(Handle.QueueDraw);
        }
    }

    float progress;
}

class ProgressDisplayClass(GTypeEnum parent, string name, Func<nint, ProgressDisplay> constructor)
    : SubClass<DrawingAreaHandle>(parent, name, constructor)
{ }

