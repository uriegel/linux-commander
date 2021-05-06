use std::cell::RefCell;

use gtk::{Application, GtkApplicationExt, GtkWindowExt, WidgetExt, Window};

use crate::settings::{Settings};

pub struct MainWindow {
    window: Window
}

impl MainWindow {
    pub fn new(window: Window) -> Self {

        let main_window = MainWindow { window };
        main_window.conncet_handlers();

        //let weak_window = window.clone();
        //weak_window.
//        let mut setts = settings;
        


        main_window
    }

    pub fn add_to_application(&self, application: &Application) {
        application.add_window(&self.window);
    }

    //pub fn initialize(&self, settings: &RefCell<Settings>) {
    pub fn initialize(&self) {
        // let current_settings = settings.borrow();
        // if current_settings.width != 0 {
        //     self.window.set_default_size(current_settings.width, current_settings.height);
        // }
    }

    pub fn show_all(&self) {
        self.window.show_all();
    }

    fn conncet_handlers(&self) {

        let wh = RefCell::new((0, 0));

        let weak_window = self.window.clone();
        self.window.connect_configure_event(move |_,_| {
            let size = weak_window.get_size();
            let old_wh = wh.replace(size);
            if size.0 != old_wh.0 || size.1 != old_wh.1 {
                println!("Anderst");
            }

            //whb.zahl = 67;
            //self.window
            //width = 99;
            // let new_settings = Settings { width: size.0, height: size.1, theme: settings.theme.clone() };
            // setts.height = 9;
            // //let old_settings = settings.height = 9; replace(new_settings);
            //if old_settings.width != size.0 || old_settings.height != size.1 {
              //  save_settings(&old_settings);
            //}
            false
        });

    }
}
