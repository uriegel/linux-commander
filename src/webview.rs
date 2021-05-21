use gio::{ActionMapExt, SimpleAction};
use glib::{ToVariant};
use gtk::{Application, Builder, GtkApplicationExt, prelude::BuilderExtManual};
use webkit2gtk::{LoadEvent, WebInspectorExt, WebView, WebViewExt};
use crate::{mainwindow::MainWindow, settings::save_theme};

const WEBMSG_TITLE: &str = "!!webmesg-title!!";

pub struct MainWebView {
    webview: WebView,
    //mainwindow: MainWindow
}

impl MainWebView {
    pub fn new(builder: &Builder, application: &Application, mainwindow: MainWindow, initial_theme: String) -> Self {
        let webview: WebView = builder.get_object("webview").unwrap();

        let initial_state = initial_theme.to_variant();
        let weak_webview = webview.clone();
    
        webview.connect_load_changed(move |_,load_event| 
            if load_event == LoadEvent::Committed {
                let script = format!("initialTheme = '{}'", initial_theme.clone());
                weak_webview.run_javascript(&script, Some(&gio::Cancellable::new()), |_|{});
            }
        );

        webview.connect_context_menu(|_, _, _, _| true );

        let weak_webview = webview.clone();
        let action = gio::SimpleAction::new("devtools", None);
        action.connect_activate(move |_,_| match weak_webview.get_inspector() {
            Some(inspector) => inspector.show(),
            None => println!("Could not show web inspector")
        });
        application.add_action(&action);
        application.set_accels_for_action("app.devtools", &["F12"]);

        let initial_bool_state = false.to_variant();
        let action = SimpleAction::new_stateful("showhidden", None, &initial_bool_state);
        let weak_webview = webview.clone();
        action.connect_change_state(move |a, s| {
            match s {
                Some(val) => {
                    a.set_state(val);
                    match val.get::<bool>(){
                        Some(show_hidden) => weak_webview.run_javascript(
                            &format!("showHidden({})", show_hidden),
                            Some(&gio::Cancellable::new()),
                            |_|{}),
                        None => println!("Could not set ShowHidden, could not extract from variant")
                    }
                },
                None => println!("Could not set theme")
            }
        });
        application.add_action(&action);
        application.set_accels_for_action("app.showhidden", &["<Ctrl>H"]);

        let action = SimpleAction::new_stateful("themes", Some(&initial_state.type_()), &initial_state);

        let clone_mainwindow = mainwindow.clone();
        webview.connect_script_dialog(move|_, dialog | {
            let str = dialog.get_message();
            if str.starts_with(WEBMSG_TITLE) {
                let msg = &str[WEBMSG_TITLE.len()..];
                    clone_mainwindow.set_sub_title(msg);
                true
            }
            else {
                false
            }
        });

        let weak_webview = webview.clone();
        action.connect_change_state(move |a, s| {
            match s {
                Some(val) => {
                    a.set_state(val);
                    match val.get_str(){
                        Some(theme) => {
                            weak_webview.run_javascript(&format!("setTheme('{}')", theme), Some(&gio::Cancellable::new()), |_|{});
                            save_theme(theme);
                        }
                        None => println!("Could not set theme, could not extract from variant")
                    }
                },
                None => println!("Could not set theme")
            }
        });
        application.add_action(&action);

        MainWebView{ 
            webview, 
            // mainwindow 
        }
    }

    pub fn load(&self, port: u16) {
        let uri = format!("http://localhost:{}", port);
        self.webview.load_uri(&uri);
    }
}
