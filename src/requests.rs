use std::future::Future;

use glib::MainContext;
use webkit2gtk::{WebView, traits::WebViewExt};

pub fn connect_msg_callback<F, R, RFut>(main_context: MainContext, webview: &WebView, on_msg: F, on_request: R) 
    where F: Fn(&str, &str)->() + 'static,
        R: FnOnce(&str, &str)->RFut + 'static,
        RFut: Future<Output = ()> {
    let webmsg = "!!webmsg!!";
    let request = "!!request!!";

    webview.connect_script_dialog(move|_, dialog | {
        let str = dialog.get_message();
        if str.starts_with(webmsg) {
            let msg = &str[webmsg.len()..];
            if let Some(pos) = msg.find("!!") {
                let cmd = &msg[0..pos];
                let payload = &msg[pos+2..];
                on_msg(cmd, payload);
            }
        } else if str.starts_with(request) {
            let msg = &str[request.len()..];
            if let Some(pos) = msg.find("!!") {
                let cmd = &msg[0..pos];
                let part = &msg[pos+2..];
                let (id, param) = if let Some(pos) = part.find("!!") {
                    let id = &part[0..pos];
                    let param = &part[pos+2..];
                    (id.to_string(), param.to_string())
//                    perform_request(&main_context, on_request, cmd.to_string(), );
                } else {
                    (part.to_string(), "{}".to_string())
                };
                main_context.spawn_local(async move {
                    println!("Performing request: {}, {}, {}", cmd, id, param);
                    on_request(&cmd, &param).await;
                });
        }
        }

        true
    });
}

fn perform_request<R, RFut>(main_context: &MainContext, on_request: R, cmd: String, id: String, param: String) 
where R: FnOnce(&str, &str)->RFut  + 'static, RFut: Future<Output = ()> {
    main_context.spawn_local(async move {
        println!("Performing request: {}, {}, {}", cmd, id, param);
        on_request(&cmd, &param).await;
        //on_request(&cmd, &param).await;
    });
}

// fn perform_request<F: FnOnce(&str, &str)->(dyn Future<Output = ()> + 'static)>(main_context: &MainContext, on_request: F, cmd: String, id: String, param: String) {
//     main_context.spawn_local(async move {
//         println!("Performing request: {}, {}, {}", cmd, id, param);
//         on_request(cmd, param).await;
//         //on_request(&cmd, &param).await;
//     });
// }

// async fn on_request(cmd: &str, param: &str)/*->Result<T, Error>*/  {
//     println!("Request: {}, {}", cmd, param);
//     match cmd {
//         "getRoot" => 
//     }
    
    
    
    
    
//     //    on_request(cmd, param).await;

// }


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

// const ICON_SIZE: i32 = 16;


// #[derive(Serialize)]
// #[serde(rename_all = "camelCase")]
// pub struct DirectoryItems {
//     pub files: Vec<FileItem>,
//     pub dirs: Vec<DirItem>
// }

// #[derive(Deserialize)]
// pub struct GetIcon {
//     ext: String,
// }

// #[derive(Deserialize)]
// pub struct GetView {
//     pub path: String
// }


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



// pub fn get_directory_items(path: &str, id: &str, suppress_hidden: bool, event_sinks: EventSinks)->Result<DirectoryItems, Error> {
//     let entries = fs::read_dir(path)?;
//     event_sinks.set_request(id, true);
//     let (dirs, files): (Vec<_>, Vec<_>) = entries
//         .filter_map(|entry| {
//             entry.ok()
//                 .and_then(|entry| { match entry.metadata().ok() {
//                     Some(metadata) => Some((entry, metadata)),
//                     None => None
//                 }})
//                 .and_then(|(entry, metadata)| {
//                     let name = String::from(entry.file_name().to_str().unwrap());
//                     let is_hidden = is_hidden(path, &name);
//                     Some(match metadata.is_dir() {
//                         true => FileType::Dir(DirItem {
//                             name,
//                             is_hidden,
//                             is_directory: true
//                         }),
//                         false => FileType::File(FileItem {
//                             name,
//                             is_hidden,
//                             time: metadata.modified().unwrap().duration_since(UNIX_EPOCH).unwrap().as_millis(),
//                             size: metadata.len()
//                         })
//                     })
//                 })
//                 .and_then(get_supress_hidden(suppress_hidden))
//         })
//         .partition(|entry| if let FileType::Dir(_) = entry { true } else {false });
//     let mut dirs: Vec<DirItem> = dirs
//         .into_iter()
//         .filter_map(|ft|if let FileType::Dir(dir) = ft {Some(dir)} else {None})
//         .collect();
//     dirs.sort_by(|a, b|natural_lexical_cmp(&a.name, &b.name));
//     let mut files: Vec<FileItem> = files
//         .into_iter()
//         .filter_map(|ft|if let FileType::File(file) = ft {Some(file)} else {None})
//         .collect();
//     files.sort_by(|a, b|natural_lexical_cmp(&a.name, &b.name));
    
//     event_sinks.set_request(id, false);

//     Ok(DirectoryItems{
//         dirs,
//         files
//     })
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

// pub async fn get_icon(param: GetIcon) -> Result<impl warp::Reply, warp::Rejection> {
//     let bytes = systemicons::get_icon(&param.ext, ICON_SIZE).unwrap();
//     let body = hyper::Body::from(bytes);
//     let mut response = Response::new(body);
//     let headers = response.headers_mut();
//     let mut header_map = create_headers();
//     header_map.insert("Content-Type", HeaderValue::from_str("image/png").unwrap());
//     headers.extend(header_map);
//     Ok (response)        
// }

// pub async fn get_view(param: GetView) -> Result<impl warp::Reply, warp::Rejection> {
//     if let Some(ext_pos) = param.path.rfind(".") {
//         let ext = &param.path[ext_pos+1..].to_lowercase();        
//         match ext.as_str() {
//             "jpg" | "png" => {
//                 match tokio::fs::File::open(param.path).await {
//                     Ok(file) => {
//                         let stream = FramedRead::new(file, BytesCodec::new());
//                         let body = hyper::Body::wrap_stream(stream);
//                         Ok (warp::reply::Response::new(body))
//                     },
//                     Err(err) => {
//                         println!("Could not get img: {}", err);
//                         Err(warp::reject())
//                     }
//                 }
//             },
//             "pdf" => {
//                 match tokio::fs::File::open(param.path).await {
//                     Ok(file) => {
//                         let stream = FramedRead::new(file, BytesCodec::new());
//                         let body = hyper::Body::wrap_stream(stream);
//                         let mut response = warp::reply::Response::new(body);
//                         let headers = response.headers_mut();
//                         let mut header_map = create_headers();
//                         header_map.insert("Content-Type", HeaderValue::from_str("application/pdf").unwrap());
//                         headers.extend(header_map);
//                         Ok (response)
//                     },
//                     Err(err) => {
//                         println!("Could not get pdf: {}", err);
//                         Err(warp::reject())
//                     }
//                 }
//             },
//             "mp4"|"mkv" => {
//                 match tokio::fs::File::open(param.path).await {
//                     Ok(file) => {
//                         let stream = FramedRead::new(file, BytesCodec::new());
//                         let body = hyper::Body::wrap_stream(stream);
//                         let mut response = warp::reply::Response::new(body);
//                         let headers = response.headers_mut();
//                         let mut header_map = create_headers();
//                         header_map.insert("Content-Type", HeaderValue::from_str("video/mp4").unwrap());
//                         headers.extend(header_map);
//                         Ok (response)
//                     },
//                     Err(err) => {
//                         println!("Could not get pdf: {}", err);
//                         Err(warp::reject())
//                     }
//                 }
//             },
//             _ => Err(warp::reject())
//         }
//     } else {
//         Err(warp::reject())
//     }
// }

// pub async fn get_video(param: GetView) -> Result<impl warp::Reply, warp::Rejection> {
//     get_range("".to_string(), &param.path, "video/mp4").await
// }

// pub async fn get_video_range(param: GetView, range_header: String) -> Result<impl warp::Reply, warp::Rejection> {
//     get_range(range_header, &param.path, "video/mp4").await
// }

// fn create_headers() -> HeaderMap {
//     let mut header_map = HeaderMap::new();
//     let now = Utc::now();
//     let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
//     header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
//     header_map.insert("Server", HeaderValue::from_str("My Server").unwrap());
//     header_map
// }


