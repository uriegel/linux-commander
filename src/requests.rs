// use chrono::{Local, NaiveDateTime, TimeZone, Utc};
// use exif::{In, Tag};
// use lexical_sort::natural_lexical_cmp;
// use serde::{Serialize, Deserialize};
// use tokio_util::codec::{BytesCodec, FramedRead};
// use warp::{http::HeaderValue, hyper::{self, HeaderMap, Response}};
// use warp_range::get_range;
// use std::{fmt, fs, iter::Take, thread::{self, sleep}, time::{Duration, UNIX_EPOCH}};

// use crate::{eventsink::EventSinks, linux::requests::is_hidden};

// #[cfg(target_os = "linux")]
// use crate::linux::requests::{check_extended_items, get_version};
// #[cfg(target_os = "windows")]
// use crate::windows::requests::{check_extended_items, get_version};



// #[derive(Serialize)]
// pub enum MsgType {
//     ExtendedItem,
//     Refresh
// }

// #[derive(Serialize)]
// #[serde(rename_all = "camelCase")]
// pub struct ExtendedItems {
//     msg_type: MsgType,
//     items: Vec<ExtendedItem>
// }

// #[derive(Serialize)]
// pub struct ExtendedItem {
//     index: usize,
//     #[serde(skip_serializing_if = "Option::is_none")]
//     #[serde(default)]
//     exiftime: Option<i64>
// }

// impl ExtendedItem {
//     pub fn new(index: usize, exiftime: i64) -> Self {
//         ExtendedItem {
//             index, 
//             exiftime: Some(exiftime)
//         }
//     }
// }




// pub fn retrieve_extended_items(id: String, path: String, items: &DirectoryItems, event_sinks: EventSinks) {
//     let index_pos = items.dirs.len() + 1;
//     let files: Vec<(usize, FileItem)> = items.files
//         .iter()
//         .enumerate()
//         .map(| (index, n)| (index, FileItem{name: n.name.clone(), is_hidden: n.is_hidden, size: n.size, time: n.time}))
//         .filter(|(_, n)| {
//             let ext = n.name.to_lowercase();
//             check_extended_items(&ext)            
//         })
//         .collect();

//     fn get_unix_time(str: &str)->i64 {
//         let naive_date_time = NaiveDateTime::parse_from_str(str, "%Y-%m-%d %H:%M:%S").unwrap();
//         let datetime = Local.from_local_datetime(&naive_date_time).unwrap();
//         datetime.timestamp_millis()
//     }

//     if files.len() > 0 {
//         if let Some(request_id) = event_sinks.register_request(id.clone()) {
//             thread::spawn( move|| {
//                 let extended_items: Vec<ExtendedItem> = files.iter().filter_map(|(index, n)| {
//                     let filename = format!("{}/{}", path, n.name);

//                     let ext = n.name.to_lowercase();
//                     if ext.ends_with(".png") || ext.ends_with(".jpg") {
//                         let file = std::fs::File::open(filename).unwrap();
//                         let mut bufreader = std::io::BufReader::new(&file);
//                         let exifreader = exif::Reader::new();
        
//                         if event_sinks.is_request_active(id.clone(), request_id) {
//                             if event_sinks.active_requests() {
//                                 sleep(Duration::from_millis(500));
//                             }
                                
//                             exifreader.read_from_container(&mut bufreader).ok().and_then(|exif|{
//                                 let exiftime = match exif.get_field(Tag::DateTimeOriginal, In::PRIMARY) {
//                                     Some(info) => Some(info.display_value().to_string()),
//                                     None => match exif.get_field(Tag::DateTime, In::PRIMARY) {
//                                         Some(info) => Some(info.display_value().to_string()),
//                                         None => None
//                                     } 
//                                 };
//                                 match exiftime {
//                                     Some(exiftime) => Some(ExtendedItem::new(index + index_pos, get_unix_time(&exiftime))),
//                                     None => None
//                                 }
//                             }) 
//                         } else {
//                             None
//                         }
//                     } else {
//                         get_version(&filename, index + index_pos) 
//                     }
//                 }).collect();
                        
//                 if extended_items.len() > 0 && event_sinks.is_request_active(id.clone(), request_id) {
//                     let extended_items_msg = ExtendedItems{ items: extended_items, msg_type: MsgType::ExtendedItem };
//                     let json = serde_json::to_string(&extended_items_msg).unwrap();
//                     event_sinks.send(id, json);
//                 }
//             });
//         }
//     }
// }




