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
    }
}
public class ProgressControlClass(string name, Func<nint, ProgressControl> constructor)
    : SubClass<RevealerHandle>(GTypeEnum.Revealer, name, constructor)
{ }
