mod app;
mod server;
mod settings;
mod mainwindow;
mod webview;

use tokio::runtime::Runtime;
use app::App;

fn main() {
    let port = 9865;
    let rt = Runtime::new().unwrap();
    server::start(&rt, port);

    let app = App::new(port);    
    app.run();
}
