use std::{cell::RefCell, env};

use gio::{ActionMapExt, ApplicationExt, ApplicationFlags, SimpleAction, prelude::ApplicationExtManual};
use glib::{ToVariant, clone};
use gtk::{Application, Builder, GtkApplicationExt, prelude::BuilderExtManual};
use webkit2gtk::{LoadEvent, WebView, WebViewExt, WebInspectorExt};

use crate::{mainwindow::MainWindow, settings};

fn set_theme(webview: WebView, theme: &str) {
    webview.run_javascript(&format!("setTheme('{}')", theme), Some(&gio::Cancellable::new()), |_|{});
}

pub struct App {
    application: Application
}

impl App {
    pub fn new(port: u16) -> Self {

        let application = Application::new(Some("de.uriegel.commander"), ApplicationFlags::empty())
            .expect("Application::new() failed");

        let action = SimpleAction::new("destroy", None);
        let weak_application = application.clone();
        action.connect_activate(move |_,_| weak_application.quit());
        application.add_action(&action);
        application.set_accels_for_action("app.destroy", &["<Ctrl>Q"]);

        unsafe {
            webkit2gtk_sys::webkit_web_view_get_type();
            webkit2gtk_sys::webkit_settings_get_type();
        }

        application.connect_startup(move |application| {
            let settings = RefCell::new(settings::initialize());
            let initial_theme;
            {
                initial_theme  = settings.borrow().theme.clone();
            }

            let builder = Builder::new();
            builder.add_from_file("main.glade").unwrap();
            let main_window = MainWindow::new(builder.get_object("window").unwrap(), settings);
            let webview: WebView = builder.get_object("webview").unwrap();
            let uri = format!("http://localhost:{}", port);
            webview.load_uri(&uri);
    
            let initial_state = initial_theme.to_variant();
    
            let weak_webview = webview.clone();
    
            webview.connect_load_changed(move |_,load_event| 
                if load_event == LoadEvent::Committed {
                    let script = format!("initialTheme = '{}'", initial_theme);
                    weak_webview.run_javascript(&script, Some(&gio::Cancellable::new()), |_|{});
                }
            );
            
            let action = SimpleAction::new_stateful("themes", Some(&initial_state.type_()), &initial_state);
            action.connect_change_state(clone!(@weak webview => move |a, s| {
                match s {
                    Some(val) => {
                        a.set_state(val);
                        match val.get_str(){
                            Some(theme) => set_theme(webview, theme),
                            None => println!("Could not set theme, could not extract from variant")
                        }
                    },
                    None => println!("Could not set theme")
                }
            }));
            application.add_action(&action);
    
            let action = gio::SimpleAction::new("devtools", None);
            action.connect_activate(clone!(@weak webview => move |_,_| match webview.get_inspector() {
                Some(inspector) => inspector.show(),
                None => println!("Could not show web inspector")
            }));
            application.add_action(&action);
            application.set_accels_for_action("app.devtools", &["F12"]);
    
            webview.connect_context_menu(|_, _, _, _| true );
    
            main_window.add_to_application(application);
            main_window.initialize(&settings);
            main_window.show_all();
        });
    
        application.connect_activate(|_| {});
            
        App { application }
    }

    pub fn run(&self) {
        self.application.run(&env::args().collect::<Vec<_>>());
    }
}   
