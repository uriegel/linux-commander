using System.Runtime.InteropServices;
using GtkDotNet.SafeHandles;
using CsTools.Extensions;

namespace GtkDotNet;

public static class WebKitWebContext
{
    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_web_context_get_default", CallingConvention = CallingConvention.Cdecl)]
    public extern static WebKitWebContextHandle GetDefault();

    public static WebKitWebContextHandle RegisterUriScheme(this WebKitWebContextHandle context, string scheme, Action<WebkitUriSchemeRequestHandle> callback)
        => context._RegisterUriScheme(scheme, request => callback(new WebkitUriSchemeRequestHandle(request)));


    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_web_context_get_security_manager", CallingConvention = CallingConvention.Cdecl)]
    public extern static WebKitSecurityManagerHandle GetSecurityManager(this WebKitWebContextHandle context);
                        
    static WebKitWebContextHandle _RegisterUriScheme(this WebKitWebContextHandle context, string scheme, CustomSchemeRequestDelegate callback)
    {
        GtkDelegates.Add(callback);
        return context.SideEffect(c => c.RegisterUriScheme(scheme, Marshal.GetFunctionPointerForDelegate((Delegate)callback)));
    }

    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_web_context_register_uri_scheme", CallingConvention = CallingConvention.Cdecl)]
    extern static void RegisterUriScheme(this WebKitWebContextHandle context, string scheme, IntPtr callback, nint _ = 0, nint __ = 0);
}