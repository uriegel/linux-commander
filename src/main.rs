use std::{cell::RefCell, env};
use gio::{ActionMapExt, ApplicationFlags, prelude::{ApplicationExt, ApplicationExtManual}};
use glib::clone;
use gtk::{Application, Builder, GtkApplicationExt, GtkWindowExt, WidgetExt, prelude::BuilderExtManual};
use webkit2gtk::{WebView, WebViewExt, WebInspectorExt};
use tokio::runtime::Runtime;

use crate::settings::{Settings, save_settings};

mod server;
mod settings;

fn main() {
    let application = Application::new(Some("de.uriegel.commander"), ApplicationFlags::empty())
        .expect("Application::new() failed");

    let port = 9865;
    let rt = Runtime::new().unwrap();
    server::start(&rt, port);

    let action = gio::SimpleAction::new("destroy", None);
    action.connect_activate(clone!(@weak application => move |_,_| application.quit()));
    application.add_action(&action);
    application.set_accels_for_action("app.destroy", &["<Ctrl>Q"]);

    unsafe {
        webkit2gtk_sys::webkit_web_view_get_type();
        webkit2gtk_sys::webkit_settings_get_type();
    }
    
    application.connect_startup(move |application| {
        let settings = RefCell::new(settings::initialize());
        let builder = Builder::new();
        builder.add_from_file("main.glade").unwrap();
        let window: gtk::Window = builder.get_object("window").unwrap();
        let webview: WebView = builder.get_object("webview").unwrap();
        let uri = format!("http://localhost:{}", port);
        webview.load_uri(&uri);

        let action = gio::SimpleAction::new("devtools", None);
        action.connect_activate(clone!(@weak webview => move |_,_| match webview.get_inspector() {
            Some(inspector) => inspector.show(),
            None => println!("Could not show web inspector")
        }));
        application.add_action(&action);
        application.set_accels_for_action("app.devtools", &["F12"]);

        webview.connect_context_menu(|_, _, _, _| true );

        application.add_window(&window);
        {
            let current_settings = settings.borrow();
            if current_settings.width != 0 {
                window.set_default_size(current_settings.width as i32, current_settings.height as i32);
            }
        }

        window.connect_configure_event(clone!(@weak window => @default-return false, move|_,_| {
            let size = window.get_size();
            let new_settings = Settings { width: size.0, height: size.1 };
            let old_settings = settings.replace(new_settings);
            if old_settings.width != size.0 || old_settings.height != size.1 {
                save_settings(&old_settings);
            }
            false
        }));

        window.show_all();
    });
    
    application.connect_activate(|_| {});
    application.run(&env::args().collect::<Vec<_>>());
}
