using System.Runtime.InteropServices;
using CsTools.Extensions;
using GtkDotNet.SafeHandles;

namespace GtkDotNet;

public static class SoupMessageHeaders
{
    [DllImport(Libs.LibWebKit, EntryPoint = "soup_message_headers_new", CallingConvention = CallingConvention.Cdecl)]
    public extern static SoupMessageHeadersNewHandle New(SoupMessageHeaderType type);

    public static IEnumerable<MessageHeader> Get(this SoupMessageHeadersHandle headers)
    {
        List<MessageHeader> headerList = new();
        headers.Foreach((h, v) => headerList.Add(new(h, v)));
        return headerList;
    }

    public static SoupMessageHeadersHandle Set(this SoupMessageHeadersHandle headers, IEnumerable<MessageHeader> headerList)
        => headers.SideEffect(h => headerList.ForEach(hv => h.Append(hv.Key, hv.Value)));

    [DllImport(Libs.LibWebKit, EntryPoint = "soup_message_headers_append", CallingConvention = CallingConvention.Cdecl)]
    extern static void Append(this SoupMessageHeadersHandle headers, string key, string value);

    [DllImport(Libs.LibWebKit, EntryPoint = "soup_message_headers_foreach", CallingConvention = CallingConvention.Cdecl)]
    extern static void Foreach(this SoupMessageHeadersHandle headers, SoupMessageHeadersDelegate foreachHeader);
}

public record MessageHeader(string Key, string Value);
