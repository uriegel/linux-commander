use lexical_sort::natural_lexical_cmp;
use serde::{Serialize};
use std::{fmt, fs, iter::Take, time::UNIX_EPOCH};

pub struct Error {
    pub message: String
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "({})", self.message)
    }
}

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

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirItem {
    name: String,
    is_directory: bool
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileItem {
    name: String,
    time: u128,
    size: u64
}

pub fn get_directory_items(path: &str)->Result<DirectoryItems, Error> {
    match fs::read_dir(path) {
        Ok(entries) => {
            let (dirs, files): (Vec<_>, Vec<_>) = entries
                .filter_map(|entry| {
                    match entry {
                        Ok(entry) => 
                            match entry.metadata() {
                                Ok(metadata) => Some(match metadata.is_dir() {
                                    true => FileType::Dir(DirItem {
                                        name: String::from(entry.file_name().to_str().unwrap()),
                                        is_directory: true
                                    }),
                                    false => FileType::File(FileItem {
                                        name: String::from(entry.file_name().to_str().unwrap()),
                                        time: metadata.modified().unwrap().duration_since(UNIX_EPOCH).unwrap().as_millis(),
                                        size: metadata.len()
                                    })
                                }),
                                _ => None
                            },
                        _ => None
                    }
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
           
            Ok(DirectoryItems{
                dirs,
                files
            })
        },
        Err(err) => Err(Error {message: format!("read_dir of {} failed: {}", path, err)})
    }
}

// use core::fmt;
// use std::{ffi::{CStr, CString, c_void}, fs, iter::Take, process::Command, time::UNIX_EPOCH, u128};
// use chrono::{Local, NaiveDateTime, TimeZone};
// use exif::{In, Tag};
// use gio_sys::GThemedIcon;
// use glib::{gobject_sys::g_object_unref, object::GObject};
// use glib_sys::g_free;
// use gtk_sys::{GtkIconTheme, gtk_icon_info_get_filename, gtk_icon_theme_choose_icon, gtk_icon_theme_get_default};
// use lexical_sort::{natural_lexical_cmp};
// use serde::{Serialize, Deserialize};

// static mut DEFAULT_THEME: Option<*mut GtkIconTheme> = None;



// #[derive(Serialize, Deserialize)]
// pub struct ExifItem {
//     index: u32,
//     name: String,
//     #[serde(default)]
//     exiftime: i64
// }


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





// pub fn get_exif_items(path: &str, items: &Vec<ExifItem>)->Result<Vec<ExifItem>, Error> {

//     fn get_unix_time(str: &str)->i64 {
//         let naive_date_time = NaiveDateTime::parse_from_str(str, "%Y-%m-%d %H:%M:%S").unwrap();
//         let datetime = Local.from_local_datetime(&naive_date_time).unwrap();
//         datetime.timestamp_millis()
//     }

//     let result: Vec<ExifItem> = items.iter().filter_map(|n | {
//         let filename = format!("{}/{}", path, n.name);
//         let file = std::fs::File::open(filename).unwrap();
//         let mut bufreader = std::io::BufReader::new(&file);
//         let exifreader = exif::Reader::new();
//         if let Ok(exif) = exifreader.read_from_container(&mut bufreader) {
//             let exiftime = match exif.get_field(Tag::DateTimeOriginal, In::PRIMARY) {
//                 Some(info) => Some(info.display_value().to_string()),
//                 None => match exif.get_field(Tag::DateTime, In::PRIMARY) {
//                     Some(info) => Some(info.display_value().to_string()),
//                     None => None
//                 } 
//             };
//             match exiftime {
//                 Some(exiftime) => Some(ExifItem { 
//                     name: n.name.to_string(), 
//                     index: n.index, 
//                     exiftime: get_unix_time(&exiftime)
//                 }),
//                 None => None
//             }
//         }
//         else {
//             None
//         }
//     }).collect();
//     Ok(result)
// }

// pub fn get_icon(ext: &str) -> String {
//     let result: String;
//     unsafe {
//         let filename = CString::new(ext).unwrap();
//         let null: u8 = 0;
//         let p_null = &null as *const u8;
//         let nullsize: usize = 0;
//         let mut res = 0;
//         let p_res = &mut res as *mut i32;
//         let p_res = gio_sys::g_content_type_guess(filename.as_ptr(), p_null, nullsize, p_res);
//         let icon = gio_sys::g_content_type_get_icon(p_res);
//         g_free(p_res as *mut c_void);
//         if DEFAULT_THEME.is_none() {
//             DEFAULT_THEME = Some(gtk_icon_theme_get_default());
//         }
//         let icon_names = gio_sys::g_themed_icon_get_names(icon as *mut GThemedIcon) as *mut *const i8;
//         let icon_info = gtk_icon_theme_choose_icon(DEFAULT_THEME.unwrap(), icon_names, 16, 2);
//         let filename = gtk_icon_info_get_filename(icon_info);
//         let res_str = CStr::from_ptr(filename);
//         result = match res_str.to_str() {
//             Ok(str) => str.to_string(),
//             Err(err) => {
//                 println!("Could not expand icon file name: {}", err);
//                 "".to_string()
//             }
//         };
//         g_object_unref(icon as *mut GObject);
//     }
//     result
// }