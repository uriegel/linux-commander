mod gtk_async;
mod warp;

use core::fmt;
use std::{cell::RefCell, fs, iter::Take, time::UNIX_EPOCH};

use async_process::Command;
use gio::{MemoryInputStream, Resource, ResourceLookupFlags, Settings, SimpleAction, prelude::ApplicationExtManual, resources_register, traits::{
        ActionMapExt, ApplicationExt, SettingsExt
    }};
use glib::{Bytes, MainContext, ToVariant};
use gtk::{
    Application, ApplicationWindow, Builder, HeaderBar, STYLE_PROVIDER_PRIORITY_APPLICATION, StyleContext, gdk::Screen, prelude::{
        BuilderExtManual, CssProviderExt, GtkApplicationExt, GtkWindowExt, HeaderBarExt, WidgetExt
    }
};
use lexical_sort::natural_lexical_cmp;
use serde::{Serialize, Deserialize};
use tokio::runtime::Runtime;
use webkit2gtk::{WebView, traits::{URISchemeRequestExt, WebContextExt, WebInspectorExt, WebViewExt}};

use crate::gtk_async::GtkFuture;

#[derive(Debug)]
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct GetItems {
    folder_id: String,
    path: String,
    #[serde(default)]
    hidden_included: bool
}

fn main() {
    let application = Application::new(Some("de.uriegel.commander"), Default::default());
    let rt = Runtime::new().unwrap();
    application.connect_activate(move|app| {
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

        let builder = Builder::from_resource("/de/uriegel/commander/main_window.glade");
        let window: ApplicationWindow = builder.object("window").expect("Couldn't get window");

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
        let action = SimpleAction::new_stateful("themes", Some(&initial_state.type_()), &initial_state);
        let webview_clone = webview.clone();
        action.connect_change_state(move |a, s| {
            match s {
            Some(val) => {
                a.set_state(val);
                match val.str(){
                    Some(theme) => 
                    webview_clone.run_javascript(&format!("setTheme('{}')", theme), Some(&gio::Cancellable::new()), |_|{}),
                    None => println!("Could not set theme, could not extract from variant")
                }
            },
            None => println!("Could not set theme")
        }
        });
        app.add_action(&action);

        let initial_bool_state = false.to_variant();
        let action = SimpleAction::new_stateful("showhidden", None, &initial_bool_state);
        let webview_clone = webview.clone();
        action.connect_change_state(move |a, s| {
            match s {
                Some(val) => {
                    a.set_state(val);
                    match val.get::<bool>(){
                        Some(show_hidden) => webview_clone.run_javascript(
                            &format!("showHidden({})", show_hidden),
                            Some(&gio::Cancellable::new()),
                            |_|{}),
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
                "theme" => action.set_state(&payload.to_variant()),
                _ => {}
            }
        }, move |cmd, id, param| {
            let main_context = MainContext::default();
            let cmd = cmd.to_string();
            let param = param.to_string();
            let id = id.to_string();
            let webview_clone = webview_clone.clone();
            main_context.spawn_local(async move {
                match cmd.as_str() {
                    "getRoot" => {
                        let items = get_root_items().await.unwrap();
                        send_request_result(&webview_clone, &id, items);
                    },                    
                    "getItems" => {
                        let params: GetItems = serde_json::from_str(&param).unwrap();
                        let erg = GtkFuture::new(move || {
                            get_directory_items(&params.path, &params.folder_id, !params.hidden_included).unwrap()
                        }).await;
                        send_request_result(&webview_clone, &id, &erg);
                    },
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

        warp::start(&rt);
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

fn send_request_result<T>(webview: &WebView, id: &str, result: T)
where T: Serialize {
    let json = serde_json::to_string(&result).expect("msg");
    webview.run_javascript(&format!("requestResult({}, {})", id, json),
    Some(&gio::Cancellable::new()),|_|{});
}

async fn get_root_items()-> Result<Vec<RootItem>, Error> {
    let output = Command::new("lsblk")
        .arg("--bytes")
        .arg("--output")
        .arg("SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE")
        .output().await.map_err(|e| Error{message: e.to_string()})?;
    if output.status.success() {
        let lines = String::from_utf8(output.stdout)
            .map_err(|e| Error{message: e.to_string()})?;

        let lines: Vec<&str> = lines.lines().collect();

        let first_line = lines[0];

        let get_part = |key: &str| {
            match first_line.match_indices(key).next() {
                Some((index, _)) => index as u16,
                None => 0
            }
        };

        let column_positions = [
            0u16, 
            get_part("NAME"),
            get_part("LABEL"),
            get_part("MOUNT"),
            get_part("FSTYPE")
        ];

        let get_string = |line: &str, pos1, pos2| {
            let index = column_positions[pos1] as usize;
            let len = match pos2 {
                | Some(pos) => Some(column_positions[pos] as usize - index),
                | None => None
            }; 
            let result: String = line
                .chars()
                .into_iter()
                .skip(index)
                .take_option(len)
                .collect();
            result
                .trim()
                .to_string()
        };

        let mut items: Vec<RootItem>= lines
            .iter()
            .skip(1)
            .map(|n| {
                let name = get_string(n, 1, Some(2));
                match name.bytes().next() {
                    Some(b) if b > 127 => {
                        let display = get_string(n, 2, Some(3));
                        let mount_point = get_string(n, 3, Some(4));
                        let capacity = match str::parse::<u64>(&get_string(n, 0, Some(1))) {
                            Ok(val) => val,
                            _ => 0
                        };
                        let file_system = get_string(n, 4, None);
                        Some(RootItem { name: name[6..].to_string(), display, mount_point, capacity, file_system })
                    },
                    _ => None
                }
            })
            .filter(|item| item.is_some())
            .map(|item|item.unwrap())
            .collect();
        let mut result = Vec::<RootItem>::new();
        if let Some(home) = dirs::home_dir() { 
            let home_item = RootItem {
                name: "~".to_string(),
                display: "".to_string(),
                mount_point: home.to_str().unwrap().to_string(),
                capacity: 0,
                file_system: "".to_string()
            };
            result.push(home_item);
        }
        result.append(&mut items);
        Ok(result)
    }
    else { 
        Err(Error {message: "Execution of lsblk failed".to_string()}) 
    }
}

pub fn get_directory_items(path: &str, _id: &str, suppress_hidden: bool)->Result<DirectoryItems, Error> {
    let entries = fs::read_dir(path)?;
//    event_sinks.set_request(id, true);
    let (dirs, files): (Vec<_>, Vec<_>) = entries
        .filter_map(|entry| {
            entry.ok()
                .and_then(|entry| { match entry.metadata().ok() {
                    Some(metadata) => Some((entry, metadata)),
                    None => None
                }})
                .and_then(|(entry, metadata)| {
                    let name = String::from(entry.file_name().to_str().unwrap());
                    let is_hidden = is_hidden(path, &name);
                    Some(match metadata.is_dir() {
                        true => FileType::Dir(DirItem {
                            name,
                            is_hidden,
                            is_directory: true
                        }),
                        false => FileType::File(FileItem {
                            name,
                            is_hidden,
                            time: metadata.modified().unwrap().duration_since(UNIX_EPOCH).unwrap().as_millis(),
                            size: metadata.len()
                        })
                    })
                })
                .and_then(get_supress_hidden(suppress_hidden))
        })
        .partition(|entry| if let FileType::Dir(_) = entry { true } else {false });
    let mut dirs: Vec<DirItem> = dirs
        .into_iter()
        .filter_map(|ft|if let FileType::Dir(dir) = ft {Some(dir)} else {None})
        .collect();
    dirs.sort_by(|a, b|natural_lexical_cmp(&a.name, &b.name));
    let mut files: Vec<FileItem> = files
        .into_iter()
        .filter_map(|ft|if let FileType::File(file) = ft {Some(file)} else {None})
        .collect();
    files.sort_by(|a, b|natural_lexical_cmp(&a.name, &b.name));
    
    // event_sinks.set_request(id, false);

    Ok(DirectoryItems{
        dirs,
        files
    })
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub display: String,
    pub mount_point: String,
    pub capacity: u64,
    pub file_system: String,
}

#[derive(Debug)]
pub struct Error {
    pub message: String
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "({})", self.message)
    }
}

impl From<std::io::Error> for Error {
    fn from(error: std::io::Error) -> Self {
        Error {message: format!("read_dir failed: {}", error)}
    }
}

pub trait IteratorExt: Iterator {

    fn take_option(self, n: Option<usize>) -> Take<Self>
        where
            Self: Sized {
        match n {
            Some(n ) => self.take(n),
            None => self.take(usize::MAX)
        }
    }
}

impl<I: Iterator> IteratorExt for I {}

pub fn is_hidden(_: &str, name: &str)->bool {
    name.as_bytes()[0] == b'.' && name.as_bytes()[1] != b'.'
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirectoryItems {
    pub files: Vec<FileItem>,
    pub dirs: Vec<DirItem>
}

enum FileType {
    Dir(DirItem),
    File(FileItem)
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileItem {
    name: String,
    is_hidden: bool,
    time: u128,
    size: u64
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirItem {
    name: String,
    is_hidden: bool,
    is_directory: bool
}

fn get_supress_hidden(supress: bool) -> fn (FileType)->Option<FileType> {
    if supress {|file_type| {
        match file_type {
            FileType::File(ref file) => if file.is_hidden { None } else { Some(file_type) }
            FileType::Dir(ref file) => if file.is_hidden { None} else { Some(file_type) }
        }
    }} else { |e| Some(e) }
}






