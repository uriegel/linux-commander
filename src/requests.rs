use chrono::{Local, NaiveDateTime, TimeZone, Utc};
use exif::{In, Tag};
use lexical_sort::natural_lexical_cmp;
use serde::{Serialize, Deserialize};
use warp::{http::HeaderValue, hyper::{self, HeaderMap, Response}};
use std::{fmt, fs, iter::Take, thread, time::UNIX_EPOCH};

use crate::eventsink::EventSinks;

#[cfg(target_os = "linux")]
use crate::linux::requests::{check_extended_items, get_version};
#[cfg(target_os = "windows")]
use crate::windows::requests::{check_extended_items, get_version};

const ICON_SIZE: i32 = 16;

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

#[derive(Deserialize)]
pub struct GetIcon {
    ext: String,
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

#[derive(Serialize, Deserialize)]
pub struct ExtendedItem {
    index: usize,
    name: String,
    #[serde(default)]
    exiftime: i64
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

pub fn retrieve_extended_items(id: String, path: String, items: &DirectoryItems, event_sinks: EventSinks) {
    let index_pos = items.dirs.len() + 1;
    let files: Vec<(usize, FileItem)> = items.files
        .iter()
        .enumerate()
        .map(| (index, n)| (index, FileItem{name: n.name.clone(), size: n.size, time: n.time}))
        .filter(|(_, n)| {
            let ext = n.name.to_lowercase();
            check_extended_items(&ext)            
        })
        .collect();

    fn get_unix_time(str: &str)->i64 {
        let naive_date_time = NaiveDateTime::parse_from_str(str, "%Y-%m-%d %H:%M:%S").unwrap();
        let datetime = Local.from_local_datetime(&naive_date_time).unwrap();
        datetime.timestamp_millis()
    }

    if files.len() > 0 {
        if let Some(request_id) = event_sinks.register_request(id.clone()) {
            thread::spawn( move|| {
                let extended_items: Vec<ExtendedItem> = files.iter().filter_map(|(index, n)| {
                    let filename = format!("{}/{}", path, n.name);

                    let ext = n.name.to_lowercase();
                    if ext.ends_with(".png") || ext.ends_with(".jpg") {
                        let file = std::fs::File::open(filename).unwrap();
                        let mut bufreader = std::io::BufReader::new(&file);
                        let exifreader = exif::Reader::new();
        
                        if event_sinks.is_request_active(id.clone(), request_id) {
                            if let Ok(exif) = exifreader.read_from_container(&mut bufreader) {
                                let exiftime = match exif.get_field(Tag::DateTimeOriginal, In::PRIMARY) {
                                    Some(info) => Some(info.display_value().to_string()),
                                    None => match exif.get_field(Tag::DateTime, In::PRIMARY) {
                                        Some(info) => Some(info.display_value().to_string()),
                                        None => None
                                    } 
                                };
                                match exiftime {
                                    Some(exiftime) => Some(ExtendedItem { 
                                        name: n.name.to_string(), 
                                        index: index + index_pos, 
                                        exiftime: get_unix_time(&exiftime)
                                    }),
                                    None => None
                                }
                            }
                            else {
                                None
                            }
                        } else {
                            None
                        }
                    } else {
                        if let Some(p) = get_version(&filename) {
                            None
                        } else {
                            None
                        }
                    }
                }).collect();
                        
                if extended_items.len() > 0 && event_sinks.is_request_active(id.clone(), request_id) {
                    let json = serde_json::to_string(&extended_items).unwrap();
                    event_sinks.send(id, json);
                }
            });
        }
    }
}

pub async fn get_icon(param: GetIcon) -> Result<impl warp::Reply, warp::Rejection> {
    let bytes = systemicons::get_icon(&param.ext, ICON_SIZE).unwrap();
    let body = hyper::Body::from(bytes);
    let mut response = Response::new(body);
    let headers = response.headers_mut();
    let mut header_map = create_headers();
    header_map.insert("Content-Type", HeaderValue::from_str("image/png").unwrap());
    headers.extend(header_map);
    Ok (response)        
}

fn create_headers() -> HeaderMap {
    let mut header_map = HeaderMap::new();
    let now = Utc::now();
    let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
    header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
    header_map.insert("Server", HeaderValue::from_str("My Server").unwrap());
    header_map
}

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

