use std::cell::RefCell;

use gtk::{Application, GtkApplicationExt, GtkWindowExt, WidgetExt, Window};

use crate::settings::{Settings, save_settings};

pub struct MainWindow {
    window: Window
}

impl MainWindow {
    pub fn new(window: Window, settings: RefCell<Settings>) -> Self {

        let weak_window = window.clone();
        window.connect_configure_event(move|_,_| {
            let size = weak_window.get_size();
            let new_settings = Settings { width: size.0, height: size.1, theme: settings.borrow().theme.clone() };
            let old_settings = settings.replace(new_settings);
            if old_settings.width != size.0 || old_settings.height != size.1 {
                save_settings(&old_settings);
            }
            false
        });

        MainWindow { window }
    }

    pub fn add_to_application(&self, application: &Application) {
        application.add_window(&self.window);
    }

    pub fn initialize(&self, settings: &RefCell<Settings>) {
        let current_settings = settings.borrow();
        if current_settings.width != 0 {
            self.window.set_default_size(current_settings.width, current_settings.height);
        }
    }

    pub fn show_all(&self) {
        self.window.show_all();
    }
}
