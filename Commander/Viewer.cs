using GtkDotNet;
using GtkDotNet.SafeHandles;

using Commander.DataContexts;
using System.ComponentModel;
using System.Globalization;

namespace Commander;

static class Viewer
{
    public static async void Show(ApplicationWindowHandle window, WidgetHandle? viewer, bool show)
    {
        viewer?.SetVisible(show);
        if (initial)
        {
            // TODO Maximize window 
            initial = false;
            var viewerPaned = window.GetTemplateChild<PanedHandle, ApplicationWindowHandle>("viewerPaned");
            viewerPaned.SetPosition(window.GetHeight() / 2);
            webView = window.GetTemplateChild<WebViewHandle, ApplicationWindowHandle>("viewer");
            webView.LoadUri("http://localhost:20000");
            await Task.Delay(400);
            SetValues();
        }
        if (show)
        {
            MainContext.Instance.PropertyChanged += PropertyChanged;
            await Task.Delay(100);
            SetValues();
        }
        else
        {
            MainContext.Instance.PropertyChanged -= PropertyChanged;
            webView?.RunJavascript($"setPath('')"); 
        }
    }

    public static void ToggleView()
        => webView?.RunJavascript($"toggleView()");


    static void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainContext.SelectedPath) || e.PropertyName == nameof(MainContext.ExifData))
            SetValues();
    }

    static void SetValues()
    {
        if (MainContext.Instance.ExifData?.Latitude != null && MainContext.Instance.ExifData?.Longitude != null)
            webView?.RunJavascript($"setPath('{MainContext.Instance.SelectedPath}', {MainContext.Instance.ExifData?.Latitude?.ToString(CultureInfo.InvariantCulture)}, {MainContext.Instance.ExifData?.Longitude?.ToString(CultureInfo.InvariantCulture)})"); 
        else
            webView?.RunJavascript($"setPath('{MainContext.Instance.SelectedPath}')"); 
    }

    static WebViewHandle? webView;
    static bool initial = true;
}