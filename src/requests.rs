use core::fmt;
use std::{ffi::{CStr, CString, c_void}, fs, iter::Take, process::Command, time::UNIX_EPOCH, u128};
use gio_sys::GThemedIcon;
use glib::{gobject_sys::g_object_unref, object::GObject};
use glib_sys::g_free;
use gtk_sys::{GtkIconTheme, gtk_icon_info_get_filename, gtk_icon_theme_choose_icon, gtk_icon_theme_get_default};
use lexical_sort::{natural_lexical_cmp};
use serde::{Serialize};

static mut DEFAULT_THEME: Option<*mut GtkIconTheme> = None;

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub display: String,
    pub mount_point: String,
    pub capacity: u64,
    pub file_system: String,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileItems {
    pub files: Vec<FileItem>,
    pub dirs: Vec<DirItem>
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

enum FileType {
    Dir(DirItem),
    File(FileItem)
}

trait IteratorExt: Iterator {

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

pub struct Error {
    pub message: String
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "({})", self.message)
    }
}

pub fn get_root_items()->Result<Vec<RootItem>, Error> {
    let output = Command::new("lsblk")
        .arg("--bytes")
        .arg("--output")
        .arg("SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE")
        .output().map_err(|e| Error{message: e.to_string()})?;
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

pub fn get_directory_items(path: &str)->Result<FileItems, Error> {
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
           
            Ok(FileItems{
                dirs,
                files
            })
        },
        Err(err) => Err(Error {message: format!("read_dir of {} failed: {}", path, err)})
    }
}

pub fn get_icon(ext: &str) -> String {
    let result: String;
    unsafe {
        let filename = CString::new(ext).unwrap();
        let null: u8 = 0;
        let p_null = &null as *const u8;
        let nullsize: usize = 0;
        let mut res = 0;
        let p_res = &mut res as *mut i32;
        let p_res = gio_sys::g_content_type_guess(filename.as_ptr(), p_null, nullsize, p_res);
        let icon = gio_sys::g_content_type_get_icon(p_res);
        g_free(p_res as *mut c_void);
        if DEFAULT_THEME.is_none() {
            DEFAULT_THEME = Some(gtk_icon_theme_get_default());
        }
        let icon_names = gio_sys::g_themed_icon_get_names(icon as *mut GThemedIcon) as *mut *const i8;
        let icon_info = gtk_icon_theme_choose_icon(DEFAULT_THEME.unwrap(), icon_names, 16, 2);
        let filename = gtk_icon_info_get_filename(icon_info);
        let res_str = CStr::from_ptr(filename);
        result = match res_str.to_str() {
            Ok(str) => str.to_string(),
            Err(err) => {
                println!("Could not expand icon file name: {}", err);
                "".to_string()
            }
        };
        g_object_unref(icon as *mut GObject);
    }
    result
}