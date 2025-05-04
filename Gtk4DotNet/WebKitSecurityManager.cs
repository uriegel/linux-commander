using System.Runtime.InteropServices;
using CsTools.Extensions;
using GtkDotNet.SafeHandles;

namespace GtkDotNet;

public static class WebKitSecurityManager
{
    public static WebKitSecurityManagerHandle RegisterUriSchemeAsCorsEnabled(this WebKitSecurityManagerHandle request, string scheme)
        => request.SideEffect(r => r._RegisterUriSchemeAsCorsEnabled(scheme));
        
    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_security_manager_register_uri_scheme_as_cors_enabled", CallingConvention = CallingConvention.Cdecl)]
    extern static void _RegisterUriSchemeAsCorsEnabled(this WebKitSecurityManagerHandle request, string scheme);
}