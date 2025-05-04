using System.Runtime.InteropServices;
using GtkDotNet.SafeHandles;

namespace GtkDotNet;

public static class Surface
{
    [DllImport(Libs.LibGtk, EntryPoint = "gdk_drag_begin", CallingConvention = CallingConvention.Cdecl)]
    public extern static DragContextHandle DragBegin(this SurfaceHandle surface, DeviceHandle device, ContentProviderHandle provider, DragAction dragAction, double x, double y);
}
