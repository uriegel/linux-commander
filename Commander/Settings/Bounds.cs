using System.Text.Json;
using CsTools;
using CsTools.Extensions;
using GtkDotNet;
using GtkDotNet.SafeHandles;

namespace Commander.Settings;

static class WindowHandleExtensions
{
    public static THandle SaveBounds<THandle>(this THandle window, int defaultWidth, int defaultHeight)
        where THandle : WindowHandle

    {
        Bounds
           .Retrieve()
           .SideEffect(b => window.DefaultSize(b.Width ?? defaultWidth, b.Height ?? defaultHeight))
            .SideEffectIf(b => b.IsMaximized, _ => window.Maximize())
            .SideEffect(_ => window.OnClose(SaveBounds));
        return window;

        bool SaveBounds(WindowHandle window)
            => false.SideEffect(_ =>
                    Bounds
                        .Save(Bounds.Retrieve() with
                        {
                            Width = window.GetWidth(),
                            Height = window.GetHeight(),
                            IsMaximized = window.IsMaximized()
                        }));
    }
}

record Bounds(int? X, int? Y, int? Width, int? Height, bool IsMaximized)
{
    public static Bounds Retrieve()
        => Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("bounds.json")
            .ReadAllTextFromFilePath()
            ?.Deserialize<Bounds>(Json.Defaults)
            ?? new(null, null, null, null, false);

    public static void Save(Bounds bounds)
        => Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .AppendPath(Globals.AppId)
            .SideEffect(d => d.EnsureDirectoryExists())
            .AppendPath("bounds.json")
            .WriteAllTextToFilePath(bounds.Serialize(Json.Defaults));
}

