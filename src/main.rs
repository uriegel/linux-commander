use webview_app::app::{App, AppSettings, WarpSettings};
#[cfg(target_os = "linux")]
use webview_app::app::connect_msg_callback;
#[cfg(target_os = "linux")]
use gtk::GtkApplicationExt;
#[cfg(target_os = "linux")]
use gio::{ActionMapExt, SimpleAction};
#[cfg(target_os = "linux")]
use gtk::{Application, ApplicationWindow, Builder, HeaderBar, HeaderBarExt, prelude::{BuilderExtManual, ToVariant}};
#[cfg(target_os = "linux")]
use webkit2gtk::{WebView, WebViewExt};
use crate::server::server;

mod server;
mod requests;
mod eventsink;
#[cfg(target_os = "linux")]
mod linux;
#[cfg(target_os = "windows")]
mod windows;

#[cfg(target_os = "linux")]
fn on_init(application: &Application, _: &ApplicationWindow, builder: &Option<Builder>, webview: &WebView) {
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

    let initial_state = "".to_variant();
    let weak_webview = webview.clone();
    let action = SimpleAction::new_stateful("themes", Some(&initial_state.type_()), &initial_state);
    action.connect_change_state(move |a, s| {
        match s {
        Some(val) => {
            a.set_state(val);
            match val.get_str(){
                Some(theme) => 
                    weak_webview.run_javascript(&format!("setTheme('{}')", theme), Some(&gio::Cancellable::new()), |_|{}),
                None => println!("Could not set theme, could not extract from variant")
            }
        },
        None => println!("Could not set theme")
    }
    });
    application.add_action(&action);

    if let Some(builder) = builder {
        let headerbar: HeaderBar = builder.get_object("headerbar").unwrap();
        connect_msg_callback(webview, move|cmd: &str, payload: &str|{ 
            match cmd {
                "title" => headerbar.set_subtitle(Some(payload)),
                "theme" => action.set_state(&payload.to_variant()),
                _ => {}
            }
        });
    }    
}

fn run_app() {
    let port = 9865;
    let app = App::new(
        AppSettings{
            title: "Commander".to_string(),
            enable_dev_tools: true,
            warp_settings: Some(WarpSettings{
                port: port,
                init_fn: Some(server)
            }),
            window_pos_storage_path: Some("commander".to_string()),
            #[cfg(target_os = "linux")]
            url: format!("http://localhost:{}?os=linux", port),
            #[cfg(target_os = "linux")]
            on_app_init: Some(on_init),
            #[cfg(target_os = "linux")]
            application_id: "de.uriegel.commander".to_string(),
            #[cfg(target_os = "linux")]
            use_glade: true,
            ..Default::default()
        });
    app.run();
}

fn main() {
    run_app();
}



