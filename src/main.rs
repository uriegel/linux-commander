extern crate gtk;
extern crate gio;

use gio::{ActionMapExt, ApplicationFlags, prelude::ApplicationExt};
use glib::clone;
use gtk::{Application, GtkApplicationExt};

fn main() {
    let application = Application::new(Some("de.uriegel.commander"), ApplicationFlags::empty())
        .expect("Application::new() failed");

    let action = gio::SimpleAction::new("destroy", None);
    action.connect_activate(clone!(@weak application => move |_,_| application.quit()));
    application.add_action(&action);
    application.set_accels_for_action("app.destroy", &["<Ctrl>Q"]);
    
}
