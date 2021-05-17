mod app;
mod server;
mod settings;
mod mainwindow;
mod webview;
mod requests;

use exif::{In, Tag};
use tokio::runtime::Runtime;
use app::App;

fn main() {


    let file = std::fs::File::open("/home/uwe/Bilder/Fotos/Canon Neu/101CANON/IMG_0001.JPG").unwrap();
    let mut bufreader = std::io::BufReader::new(&file);
    let exifreader = exif::Reader::new();
    let exif = exifreader.read_from_container(&mut bufreader).unwrap();
    let info = exif.get_field(Tag::DateTimeOriginal, In::PRIMARY).unwrap();
    let display = info.display_value().to_string();

    let info = exif.get_field(Tag::DateTime, In::PRIMARY).unwrap();
    let display = info.display_value().to_string();

    let port = 9865;
    let rt = Runtime::new().unwrap();
    server::start(&rt, port);

    let app = App::new(port);    
    app.run();
}
