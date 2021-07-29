mod gtk_async;
mod progress;
mod requests;

use std::cell::RefCell;

use gio::{
    File, MemoryInputStream, Resource, ResourceLookupFlags, Settings, SimpleAction, prelude::ApplicationExtManual, resources_register, traits::{
        ActionMapExt, ApplicationExt, FileExt, SettingsExt
    }
};
use glib::{Bytes, MainContext, PRIORITY_DEFAULT, ToVariant};
use gtk::{
    Application, ApplicationWindow, Builder, HeaderBar, STYLE_PROVIDER_PRIORITY_APPLICATION, StyleContext, gdk::Screen, prelude::{
        BuilderExtManual, CssProviderExt, GtkApplicationExt, GtkWindowExt, HeaderBarExt, WidgetExt
    }
};
use webkit2gtk::{WebView, traits::{SecurityManagerExt, URISchemeRequestExt, WebContextExt, WebInspectorExt, WebViewExt}};

use crate::{
    gtk_async::GtkFuture, progress::Progress, requests::{
        DeleteItems, ExifItems, GetItems, MsgType, Requests, get_directory_items, get_root_items, refresh_folder, retrieve_exif_items, 
        send_exifs, send_progress, send_request_result, set_theme, show_hidden
    }
};

fn main() {
    let application = Application::new(Some("de.uriegel.commander"), Default::default());
    application.connect_activate(|app| {
        let resources_bytes = include_bytes!("../resources/resources.gresource");
        let resource_data = glib::Bytes::from(&resources_bytes[..]);
        let res = Resource::from_data(&resource_data).unwrap();
        resources_register(&res);

        let provider = gtk::CssProvider::new();
        provider.load_from_resource("/de/uriegel/commander/style.css");
        StyleContext::add_provider_for_screen(
            &Screen::default().expect("Error initializing gtk css provider."),
            &provider,
            STYLE_PROVIDER_PRIORITY_APPLICATION,
        );        

        let requests = Requests::new();
        requests.insert("folderLeft".to_string());
        requests.insert("folderRight".to_string());

        let builder = Builder::from_resource("/de/uriegel/commander/main_window.glade");
        let window: ApplicationWindow = builder.object("window").expect("Couldn't get window");
        let progress = Progress::new(&builder);

        let settings = Settings::new("de.uriegel.commander");
        let width = settings.int("window-width");
        let height = settings.int("window-height");
        let is_maximized = settings.boolean("is-maximized");
        window.set_default_size(width, height);
        if is_maximized {
            window.maximize();
        }        

        let webview: WebView = builder.object("webview").expect("Couldn't get webview");
        let headerbar: HeaderBar = builder.object("headerbar").unwrap();
        webview.connect_context_menu(|_, _, _, _| true );
        let initial_state = "".to_variant();
        let action_themes = SimpleAction::new_stateful("themes", Some(&initial_state.type_()), &initial_state);
        let webview_clone = webview.clone();
        action_themes.connect_change_state(move |a, s| {
            match s {
            Some(val) => {
                a.set_state(val);
                match val.str(){
                    Some(theme) => set_theme(&webview_clone, theme),
                    None => println!("Could not set theme, could not extract from variant")
                }
            },
            None => println!("Could not set theme")
        }
        });
        app.add_action(&action_themes);

        let initial_bool_state = false.to_variant();
        let action = SimpleAction::new_stateful("showhidden", None, &initial_bool_state);
        let webview_clone = webview.clone();
        action.connect_change_state(move |a, s| {
            match s {
                Some(val) => {
                    a.set_state(val);
                    match val.get::<bool>(){
                        Some(show) => show_hidden(&webview_clone, show),
                        None => println!("Could not set ShowHidden, could not extract from variant")
                    }
                },
                None => println!("Could not set action")
            }
        });
        app.add_action(&action);
        app.set_accels_for_action("app.showhidden", &["<Ctrl>H"]);

        let webview_clone = webview.clone();
        connect_msg_callback(&webview, move|cmd: &str, payload: &str|{ 
            match cmd {
                "title" => headerbar.set_subtitle(Some(payload)),
                "theme" => action_themes.set_state(&payload.to_variant()),
                _ => {}
            }
        }, move |cmd, id, param| {
            let main_context = MainContext::default();
            let cmd = cmd.to_string();
            let param = param.to_string();
            let id = id.to_string();
            let webview_clone = webview_clone.clone();
            let webview_clone2 = webview_clone.clone();
            let requests_clone = requests.clone();
            let progress = progress.clone();
            main_context.spawn_local(async move {
                match cmd.as_str() {
                    "getRoot" => {
                        let items = get_root_items().await.unwrap();
                        send_request_result(&webview_clone, &id, items);
                    },                    
                    "getItems" => {
                        let params: GetItems = serde_json::from_str(&param).unwrap();
                        let folder_id = params.folder_id.clone();
                        let reqid = requests_clone.register_request(&folder_id).expect("Could not get req id");
                        let path = params.path.clone();
                        let items = GtkFuture::new(move || {
                            get_directory_items(&params.path, !params.hidden_included).unwrap()
                        }).await;
                        send_request_result(&webview_clone, &id, &items);
                        
                        let folder_id_clone = folder_id;
                        let folder_id_clone2 = folder_id_clone.clone();
                        let exif_items = GtkFuture::new(move || {
                            retrieve_exif_items(&folder_id_clone, reqid, path, &items, &requests_clone)
                        }).await;
                        if let Some(exif_items) = exif_items {
                            let exif_items = ExifItems{ items: exif_items, msg_type: MsgType::ExifItem };
                            send_exifs(&webview_clone, &folder_id_clone2, &exif_items);
                        }
                    },
                    "deleteItems" => {
                        let params: DeleteItems = serde_json::from_str(&param).unwrap();
                        println!("DELETE {:?}", params);
                        let folder_id = params.folder_id.clone();

                        let files_to_delete: Vec<String> = params.items_to_delete.iter().map(|file|{
                            params.path.clone() + if params.path.ends_with("/") { "" } else { "/" } + file
                        }).collect();
    
                        let count = files_to_delete.len();
                        for (pos, filepath) in files_to_delete.iter().enumerate() {
                            let file = File::for_path(filepath);
                            // TODO:
                            let _result = file.trash_async_future(PRIORITY_DEFAULT).await;
                            send_progress(&progress.sender, count, pos + 1);
                        }
                        refresh_folder(&webview_clone2, &folder_id);
                    }
                    _ => {}
                }
            });
        });
            
        let webview_clone = webview.clone();
        let action = gio::SimpleAction::new("devtools", None);
        action.connect_activate(move |_,_| match webview_clone.inspector() {
            Some(inspector) => inspector.show(),
            None => println!("Could not show web inspector")
        });
        app.add_action(&action);        
        app.set_accels_for_action("app.devtools", &["F12"]);

        let initial_bool_state = false.to_variant();
        let action = SimpleAction::new_stateful("viewer", None, &initial_bool_state);
        let webview_clone = webview.clone();
        action.connect_change_state(move |a, s| {
            match s {
                Some(val) => {
                    a.set_state(val);
                    match val.get::<bool>(){
                        Some(show_viewer) => webview_clone.run_javascript(
                            &format!("showViewer({})", show_viewer),
                            Some(&gio::Cancellable::new()),
                            |_|{}),
                        None => println!("Could not show Viewer, could not extract from variant")
                    }
                },
                None => println!("Could not set action")
            }
        });
        app.add_action(&action);
        app.set_accels_for_action("app.viewer", &["F3"]);

        let context = webview.context().unwrap();
        fn get_content_type(path: &str)->Option<String> {
            match path {
                p if p.ends_with("js") => Some("application/javascript".to_string()),
                p if p.ends_with("css") => Some("text/css".to_string()),
                _ => None
            }
        }

        let secman = context.security_manager().expect("");
        secman.register_uri_scheme_as_local("provide");
        context.register_uri_scheme("provide", move |request|{
            let gpath = request.path().unwrap();
            let path = gpath.as_str();
            let gurl = request.uri().unwrap();
            let url = gurl.as_str();
            println!("Pfad {}  {}", path, url);
            let (subpath, content_type) = match path {
                "" => ("/index.html", Some("text/html".to_string())),
                a => (a, get_content_type(path))
            };

            if let Some(content_type) = content_type {
                let path = "/de/uriegel/commander/web".to_string() + subpath;
                let (size, _) = res.info(&path, ResourceLookupFlags::NONE).unwrap();
                let istream = res.open_stream(&path, ResourceLookupFlags::NONE).unwrap();
                request.finish(&istream, size as i64, Some(&content_type));
            }             

            if subpath == "/commander/geticon" {
                let pos = url.find("?").unwrap();
                let ext = &url[pos+5..];
                if let Some(icon) = systemicons::get_icon(ext, 16).ok() {
                    let bytes = Bytes::from_owned(icon);
                    let stream = MemoryInputStream::from_bytes(&bytes);
                    request.finish(&stream, bytes.len() as i64, Some("image/png"));
                }
            }
        });
        webview.load_uri("provide://content");

        let r_size = RefCell::new((0, 0));
        let r_is_maximized = RefCell::new(is_maximized);
        let window_clone = window.clone();
            window.connect_configure_event(move |_,_| {
            let size = window_clone.size();
            let old_r_size = r_size.replace(size);
            if size.0 != old_r_size.0 || size.1 != old_r_size.1 {
                settings.set_int("window-width", size.0).ok();
                settings.set_int("window-height", size.1).ok();                
            }
            let is_maximized = window_clone.is_maximized();
            let old_r_is_maximized = r_is_maximized.replace(is_maximized);
            if old_r_is_maximized != is_maximized {
                settings.set_boolean("is-maximized", is_maximized).ok();                
            }
            false
        });        

        window.set_application(Some(app));
        window.show_all();
    });

    application.run();
}

fn connect_msg_callback<F: Fn(&str, &str)->() + 'static, R: Fn(&str, &str, &str)->() + 'static>(webview: &WebView, on_msg: F, on_request: R) {
    let webmsg = "!!webmsg!!";
    let request = "!!request!!";

    webview.connect_script_dialog(move|_, dialog | {
        let str = dialog.get_message();
        if str.starts_with(webmsg) {
            let msg = &str[webmsg.len()..];
            if let Some(pos) = msg.find("!!") {
                let cmd = &msg[0..pos];
                let payload = &msg[pos+2..];
                on_msg(cmd, payload);
            }
        } else if str.starts_with(request) {
            let msg = &str[request.len()..];
            if let Some(pos) = msg.find("!!") {
                let cmd = &msg[0..pos];
                let part = &msg[pos+2..];
                if let Some(pos) = part.find("!!") {
                    let id = &part[0..pos];
                    let param = &part[pos+2..];
                    on_request(cmd, id, param);
                } else {
                    on_request(cmd, part, "{}");
                }
            }
        }

        true
    });
}

