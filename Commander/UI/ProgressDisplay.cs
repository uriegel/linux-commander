using GtkDotNet.SafeHandles;
using GtkDotNet.SubClassing;

namespace Commander.UI;

class ProgressDisplay(nint obj) : SubClassInst<DrawingAreaHandle>(obj)
{
    protected override void OnCreate()
    {

    }

    protected override DrawingAreaHandle CreateHandle(nint obj) => new(obj);

    protected override void OnFinalize()
    {
        Console.WriteLine("ProgressDisplay finalized");
    }
}

class ProgressDisplayClass(GTypeEnum parent, string name, Func<nint, ProgressDisplay> constructor)
    : SubClass<DrawingAreaHandle>(parent, name, constructor)
{ }

