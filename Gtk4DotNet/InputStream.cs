using System.Runtime.InteropServices;
using GtkDotNet.SafeHandles;

namespace GtkDotNet;

public static class InputStream
{
    [DllImport(Libs.LibGtk, EntryPoint = "g_input_stream_read", CallingConvention = CallingConvention.Cdecl)]
    public extern static int Read(this InputStreamHandle stream, nint buffer, int count);
}