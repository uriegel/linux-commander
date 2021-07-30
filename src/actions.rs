use glib::Sender;
use webkit2gtk::{WebView, traits::WebViewExt};

use crate::requestdata::ExifItems;

pub fn set_theme(webview: &WebView, theme: &str) {
    send_msg(webview, &format!("setTheme('{}')", theme));   
}

pub fn send_progress(sender: &Sender<f32>, count: usize, val: usize) {
    sender.send(val as f32/ count as f32).ok();
}

pub fn refresh_folder(webview: &WebView, folder_id: &str) {
    send_msg(webview, &format!("refreshFolder('{}')", folder_id));   
}

pub fn send_exifs(webview: &WebView, folder_id: &str, exif_items: &ExifItems) {
    send_msg(webview, &format!("onExifitems('{}', {})", folder_id, serde_json::to_string(&exif_items).unwrap()));   
}

pub fn show_hidden(webview: &WebView, show: bool) {
    send_msg(webview, &format!("showHidden({})", show));   
}

fn send_msg(webview: &WebView, msg: &str) {
    webview.run_javascript(msg, Some(&gio::Cancellable::new()), |_|{});   
}
