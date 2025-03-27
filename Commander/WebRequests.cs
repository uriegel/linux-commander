using CsTools;
using CsTools.Extensions;
using WebServerLight;

namespace Commander;

static class WebRequests
{
    public static Task<bool> OnGet(IRequest request) =>
        request.Url switch
        {
            var url when url.StartsWith("/getfile") => ProcessFile(request),
            _ => false.ToAsync()
        };

    static async Task<bool> ProcessFile(IRequest request)
    {
        try
        {
            var filepath = request.QueryParts.GetValue("path");
            if (filepath != null)
            {
                using var file = File.OpenRead(filepath);
                var ext = filepath?.GetFileExtension()?.ToLowerInvariant() ?? ".txt";
                var mime = ext == ".png" || ext == ".jpg" || ext == ".pdf" || ext == ".mp4" || ext == ".mkv" || ext == ".mp3"
                    ? MimeType.Get(ext)!
                    : MimeTypes.TextPlain;
                if (mime != MimeTypes.TextPlain || !file.IsBinary())
                    await request.SendAsync(file, file.Length, mime == MimeTypes.TextPlain ? MimeTypes.TextPlain + "; charset=utf-8" : mime);
                else
                    await request.Send404();

                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    static bool IsBinary(this Stream file)
    {
        file.Position = 0;
        try
        {
            const int charsToCheck = 8000;
            const char nulChar = '\0';
            var requiredConsecutiveNull = 1;

            int nulCount = 0;

            using var streamReader = new StreamReader(file, leaveOpen: true);
            for (var i = 0; i < charsToCheck; i++)
            {
                if (streamReader.EndOfStream)
                    return false;

                if ((char)streamReader.Read() == nulChar)
                {
                    nulCount++;

                    if (nulCount >= requiredConsecutiveNull)
                        return true;
                }
                else
                    nulCount = 0;
            }
            return false;
        }
        finally
        {
            file.Position = 0;
        }
    }
}