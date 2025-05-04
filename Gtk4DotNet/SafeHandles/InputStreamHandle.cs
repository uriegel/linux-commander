using CsTools.Extensions;

namespace GtkDotNet.SafeHandles;

public class InputStreamHandle : ObjectHandle
{
    public InputStreamHandle() : base() {}

    protected override bool ReleaseHandle() 
        => true.SideEffect(_ => GObject.Unref(handle));
}