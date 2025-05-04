using System.Runtime.InteropServices;

namespace GtkDotNet.SafeHandles;

public class SoupMessageHeadersHandle : BaseHandle
{
    public SoupMessageHeadersHandle() : base() {}
}

public class SoupMessageHeadersNewHandle : SoupMessageHeadersHandle
{
    public SoupMessageHeadersNewHandle() : base() {}

    protected override bool ReleaseHandle()
        => true; // .SideEffect(_ => Unref(handle)); TODO Ref Unref. Use Ref when adding header

    [DllImport(Libs.LibWebKit, EntryPoint = "soup_message_headers_unref", CallingConvention = CallingConvention.Cdecl)]
    extern static void Unref(nint headers);
}
