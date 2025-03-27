using GtkDotNet;
using GtkDotNet.SafeHandles;

using Commander.DataContexts;
using System.ComponentModel;

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
            viewerPaned?.SetPosition(window.GetHeight() / 2);
            webView = window.GetTemplateChild<WebViewHandle, ApplicationWindowHandle>("viewer");
            webView?.LoadUri("http://localhost:20000");
            await Task.Delay(400);
            webView?.RunJavascript($"setPath('{MainContext.Instance.SelectedPath}')");
        }
        if (show)
        {
            MainContext.Instance.PropertyChanged += PropertyChanged;
            await Task.Delay(100);
            webView?.RunJavascript($"setPath('{MainContext.Instance.SelectedPath}')");
        }
        else
            MainContext.Instance.PropertyChanged -= PropertyChanged;

    }

    static void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainContext.SelectedPath))
            webView?.RunJavascript($"setPath('{MainContext.Instance.SelectedPath}')");
    }

    static WebViewHandle? webView;
    static bool initial = true;
}