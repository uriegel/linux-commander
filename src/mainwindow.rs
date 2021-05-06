use std::cell::RefCell;
use gtk::{Application, Builder, GtkApplicationExt, GtkWindowExt, WidgetExt, Window, prelude::BuilderExtManual};
use crate::{settings::{self, Settings, save_settings}, webview::MainWebView};

pub struct MainWindow {
    //window: Window
}

impl MainWindow {
    pub fn new(application: &Application, port: u16) -> Self {
        let settings = settings::initialize();
        let initial_theme = settings.theme.clone();

        let builder = Builder::new();
        builder.add_from_file("main.glade").unwrap();
        let window: Window = builder.get_object("window").unwrap();
        
        let webview = MainWebView::new(&builder, application, initial_theme);
        webview.load(port);

        if settings.width != 0 {
            window.set_default_size(settings.width, settings.height);
        }

        let wh = RefCell::new((0, 0));
        let weak_window = window.clone();
        window.connect_configure_event(move |_,_| {
            let size = weak_window.get_size();
            let old_wh = wh.replace(size);
            if size.0 != old_wh.0 || size.1 != old_wh.1 {
                save_settings(Settings{width: size.0, height: size.1, theme: settings.theme.clone()});
            }
            false
        });

        application.add_window(&window);
        window.show_all();
        MainWindow {  }
    }
}
