use webview_app::app::{App, AppSettings};

//mod app;
//mod server;
mod settings;
//mod mainwindow;
//mod webview;
//mod requests;

// use gio::{ActionMapExt, SimpleAction};
// use glib::ToVariant;
// use gtk::HeaderBar;
// use tokio::runtime::Runtime;
// use app::App;
// use webkit2gtk::WebViewExt;

fn run_app() {
    let app = App::new(
        AppSettings{
            title: "Commander".to_string(),
            url: "https://crates.io".to_string(), 
            #[cfg(target_os = "linux")]
            use_glade: true,
            ..Default::default()
        });
    app.run();
}

fn main() {
    run_app();
}

// fn main() {
//     let port = 9865;
//     let rt = Runtime::new().unwrap();
//     server::start(&rt, port);

//     let app = App::new(port, |application, webview|{
//         let initial_state = "themeAdwaitaDark".to_variant();
//         let action = SimpleAction::new_stateful("themes", Some(&initial_state.type_()), &initial_state);
//         let weak_webview = webview.clone();
//         action.connect_change_state(move |a, s| {
//             match s {
//             Some(val) => {
//                 a.set_state(val);
//                 match val.get_str(){
//                     Some(theme) => {
//                         weak_webview.run_javascript(&format!("setTheme('{}')", theme), Some(&gio::Cancellable::new()), |_|{});
//                     }
//                     None => println!("Could not set theme, could not extract from variant")
//                 }
//             },
//             None => println!("Could not set theme")
//         }
//         });
//         application.add_action(&action);
        
//     });    
//     app.run();
// }


