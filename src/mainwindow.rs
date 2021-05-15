use std::{cell::RefCell, ffi::{CString, c_void}};

use gio_sys::GThemedIcon;
use glib::{glib_sys::{g_free }, gobject_sys::g_object_unref, object::GObject};
use gtk::{Application, Builder, GtkApplicationExt, GtkWindowExt, WidgetExt, Window, prelude::BuilderExtManual};
use gtk_sys::{gtk_icon_info_get_filename, gtk_icon_theme_choose_icon, gtk_icon_theme_get_default};
use crate::{settings::{initialize_size, initialize_theme, save_size}, webview::MainWebView};

pub struct MainWindow {
    //window: Window
}

impl MainWindow {
    pub fn new(application: &Application, port: u16) -> Self {
        let initial_theme = initialize_theme();
        let initial_size = initialize_size();

        let builder = Builder::new();
        builder.add_from_file("main.glade").unwrap();
        let window: Window = builder.get_object("window").unwrap();
        
        let webview = MainWebView::new(&builder, application, initial_theme);
        webview.load(port);
        window.set_default_size(initial_size.0, initial_size.1);

        let filename = CString::new(".pdf").unwrap();
        let null: u8 = 0;
        let p_null = &null as *const u8;
        let nullsize: usize = 0;
        let mut res = 0;
        let p_res = &mut res as *mut i32;
        unsafe {
            let pres = gio_sys::g_content_type_guess(filename.as_ptr(), p_null, nullsize, p_res);
            let icon = gio_sys::g_content_type_get_icon(pres);
            g_free(pres as *mut c_void);
            let theme = gtk_icon_theme_get_default();
            let icon_names = gio_sys::g_themed_icon_get_names(icon as *mut GThemedIcon) as *mut *const i8;
            let icon_info = gtk_icon_theme_choose_icon(theme, icon_names, 48, 2);
            let filename = gtk_icon_info_get_filename(icon_info) as *mut i8;
            let resstr = CString::from_raw(filename);
            let res = resstr.to_str().unwrap();
            println!("Das issses endlich: {}", res);
            g_object_unref(icon as *mut GObject);
        }



     





        let wh = RefCell::new((0, 0));
        let weak_window = window.clone();
        window.connect_configure_event(move |_,_| {
            let size = weak_window.get_size();
            let old_wh = wh.replace(size);
            if size.0 != old_wh.0 || size.1 != old_wh.1 {
                save_size((size.0,  size.1));
            }
            false
        });        

        application.add_window(&window);
        window.show_all();
        MainWindow {  }
    }
}
