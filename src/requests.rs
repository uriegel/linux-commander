use async_process::Command;
use chrono::{Local, NaiveDateTime, TimeZone};
use exif::{In, Tag};
use glib::Sender;
use lexical_sort::natural_lexical_cmp;
use serde::{Serialize, Deserialize};
use webkit2gtk::{WebView, traits::WebViewExt};
use core::fmt;
use std::{collections::HashMap, fs, iter::Take, sync::{Arc, Mutex, atomic::{AtomicUsize, Ordering}}, time::UNIX_EPOCH};

#[derive(Clone)]
pub struct Requests {
    es: Arc<Mutex<HashMap<String, AtomicUsize>>>
} 

impl Requests {
    pub fn new() -> Self {
        Self { es: Arc::new(Mutex::new(HashMap::new())) }
    }

    pub fn insert(&self, id: String) {
        self.es.lock().unwrap().insert(id, AtomicUsize::new(0));    
    }

    pub fn register_request(&self, id: &str) -> Option<usize> {
        if let Some(request_id) = self.es.lock().unwrap().get(id) {
            Some(request_id.fetch_add(1, Ordering::Relaxed) + 1)
        } else {
            None
        }
    }

    pub fn is_request_active(&self, id: &str, request_id: usize) -> bool {
        if let Some(recent_request_id) = self.es.lock().unwrap().get(id) {
            recent_request_id.load(Ordering::Relaxed) == request_id
        } else {
            false
        }
    }
}

#[derive(Debug)]
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GetItems {
    pub folder_id: String,
    pub path: String,
    #[serde(default)]
    pub hidden_included: bool
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub display: String,
    pub mount_point: String,
    pub capacity: u64,
    pub file_system: String,
}

#[derive(Debug)]
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

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileItem {
    name: String,
    is_hidden: bool,
    time: u128,
    size: u64
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirItem {
    name: String,
    is_hidden: bool,
    is_directory: bool
}

#[derive(Serialize)]
pub enum MsgType {
    ExifItem
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ExifItems {
    pub msg_type: MsgType,
    pub items: Vec<ExifItem>
}

#[derive(Debug)]
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct DeleteItems {
    pub folder_id: String,
    pub path: String,
    pub items_to_delete: Vec<String>
}

#[derive(Serialize)]
pub struct ExifItem {
    index: usize,
    #[serde(skip_serializing_if = "Option::is_none")]
    #[serde(default)]
    exiftime: Option<i64>
}

impl ExifItem {
    pub fn new(index: usize, exiftime: i64) -> Self {
        ExifItem {
            index, 
            exiftime: Some(exiftime)
        }
    }
}

#[derive(Debug)]
pub struct Error {
    pub message: String
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "({})", self.message)
    }
}

impl From<std::io::Error> for Error {
    fn from(error: std::io::Error) -> Self {
        Error {message: format!("read_dir failed: {}", error)}
    }
}

pub async fn get_root_items()-> Result<Vec<RootItem>, Error> {
    let output = Command::new("lsblk")
        .arg("--bytes")
        .arg("--output")
        .arg("SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE")
        .output().await.map_err(|e| Error{message: e.to_string()})?;
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

pub fn get_directory_items(path: &str, suppress_hidden: bool)->Result<DirectoryItems, Error> {
    let entries = fs::read_dir(path)?;
    let (dirs, files): (Vec<_>, Vec<_>) = entries
        .filter_map(|entry| {
            entry.ok()
                .and_then(|entry| { match entry.metadata().ok() {
                    Some(metadata) => Some((entry, metadata)),
                    None => None
                }})
                .and_then(|(entry, metadata)| {
                    let name = String::from(entry.file_name().to_str().unwrap());
                    let is_hidden = is_hidden(path, &name);
                    Some(match metadata.is_dir() {
                        true => FileType::Dir(DirItem {
                            name,
                            is_hidden,
                            is_directory: true
                        }),
                        false => FileType::File(FileItem {
                            name,
                            is_hidden,
                            time: metadata.modified().unwrap().duration_since(UNIX_EPOCH).unwrap().as_millis(),
                            size: metadata.len()
                        })
                    })
                })
                .and_then(get_supress_hidden(suppress_hidden))
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
}

pub fn retrieve_exif_items(folder_id: &str, reqid: usize, path: String, items: &DirectoryItems, requests: &Requests)->Option<Vec<ExifItem>> {
    let index_pos = items.dirs.len() + 1;
    let files: Vec<(usize, FileItem)> = items.files
        .iter()
        .enumerate()
        .map(| (index, n)| (index, FileItem{name: n.name.clone(), is_hidden: n.is_hidden, size: n.size, time: n.time}))
        .filter(|(_, n)| {
            let ext = n.name.to_lowercase();
            check_exif_items(&ext)            
        })
        .collect();

    fn get_unix_time(str: &str)->i64 {
        let naive_date_time = NaiveDateTime::parse_from_str(str, "%Y-%m-%d %H:%M:%S").unwrap();
        let datetime = Local.from_local_datetime(&naive_date_time).unwrap();
        datetime.timestamp_millis()
    }

    if files.len() > 0 {
        let exif_items: Vec<ExifItem> = files.iter().filter_map(|(index, n)| {
            if requests.is_request_active(folder_id, reqid) {
                let filename = format!("{}/{}", path, n.name);

                let ext = n.name.to_lowercase();
                if ext.ends_with(".png") || ext.ends_with(".jpg") {
                    let file = std::fs::File::open(filename).unwrap();
                    let mut bufreader = std::io::BufReader::new(&file);
                    let exifreader = exif::Reader::new();
                        
                    exifreader.read_from_container(&mut bufreader).ok().and_then(|exif|{
                        let exiftime = match exif.get_field(Tag::DateTimeOriginal, In::PRIMARY) {
                            Some(info) => Some(info.display_value().to_string()),
                            None => match exif.get_field(Tag::DateTime, In::PRIMARY) {
                                Some(info) => Some(info.display_value().to_string()),
                                None => None
                            } 
                        };
                        match exiftime {
                            Some(exiftime) => Some(ExifItem::new(index + index_pos, get_unix_time(&exiftime))),
                            None => None
                        }
                    }) 
                } else {
                    None
                }
            } else {
                    None
            }
        }).collect();
        if exif_items.len() > 0 && requests.is_request_active(folder_id, reqid) { 
            Some(exif_items) 
        } else { 
            None
        } 
    } else {
        None
    }
}

pub fn send_request_result<T>(webview: &WebView, id: &str, result: T)
where T: Serialize {
    let json = serde_json::to_string(&result).expect("msg");
    webview.run_javascript(&format!("requestResult({}, {})", id, json),
    Some(&gio::Cancellable::new()),|_|{});
}

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

fn is_hidden(_: &str, name: &str)->bool {
    name.as_bytes()[0] == b'.' && name.as_bytes()[1] != b'.'
}

fn get_supress_hidden(supress: bool) -> fn (FileType)->Option<FileType> {
    if supress {|file_type| {
        match file_type {
            FileType::File(ref file) => if file.is_hidden { None } else { Some(file_type) }
            FileType::Dir(ref file) => if file.is_hidden { None} else { Some(file_type) }
        }
    }} else { |e| Some(e) }
}

fn check_exif_items(ext: &str)->bool {
    ext.ends_with(".png") 
    || ext.ends_with(".jpg")
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
