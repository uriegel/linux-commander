using System.Runtime.InteropServices;
using CsTools.Extensions;
using GtkDotNet.SafeHandles;

namespace GtkDotNet;

public static class WebKitUriSchemeResponse
{
    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_uri_scheme_response_new", CallingConvention = CallingConvention.Cdecl)]
    public extern static WebKitUriSchemeResponseHandle New(InputStreamHandle stream, long length);

    public static WebKitUriSchemeResponseHandle HttpHeaders(this WebKitUriSchemeResponseHandle response, SoupMessageHeadersHandle headers)
        => response.SideEffect(r => r.SetHttpHeaders(headers));

    public static WebKitUriSchemeResponseHandle Status(this WebKitUriSchemeResponseHandle response, int status, string statusPhrase)
        => response.SideEffect(r => r.SetStatus(status, statusPhrase));

    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_uri_scheme_response_set_content_type", CallingConvention = CallingConvention.Cdecl)]
    extern static void SetContentType(this WebKitUriSchemeResponseHandle response, string contentType);

    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_uri_scheme_response_set_http_headers", CallingConvention = CallingConvention.Cdecl)]
    extern static void SetHttpHeaders(this WebKitUriSchemeResponseHandle response, SoupMessageHeadersHandle headers);

    [DllImport(Libs.LibWebKit, EntryPoint = "webkit_uri_scheme_response_set_status", CallingConvention = CallingConvention.Cdecl)]
    extern static void SetStatus(this WebKitUriSchemeResponseHandle response, int status, string statusPhrase);
}