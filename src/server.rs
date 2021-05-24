use std::net::SocketAddr;

use tokio::runtime::Runtime;
use warp::{Filter, fs::dir};
use webview_app::headers::add_headers;

use crate::linux::requests::get_root_items;

// extern crate chrono;

// use chrono::Utc;
// use warp::{Filter, Reply, fs::File, http::HeaderValue, hyper::{self, Body, HeaderMap, Response, StatusCode}};
// use tokio::runtime::Runtime;
// use tokio_util::codec::{BytesCodec, FramedRead};
// use serde::{Deserialize, Serialize};
// use crate::requests::{self, ExifItem, get_directory_items, get_exif_items, get_root_items};

// static NOTFOUND: &[u8] = b"Not Found";

// #[derive(Deserialize)]
// struct GetItems {
//     path: String,
// }

// #[derive(Deserialize)]
// struct GetExifs {
//     path: String,
//     items: Vec<ExifItem>
// }

// #[derive(Deserialize)]
// struct GetIcon {
//     ext: String,
// }

// fn create_headers() -> HeaderMap {
//     let mut header_map = HeaderMap::new();
//     let now = Utc::now();
//     let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
//     header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
//     header_map.insert("Server", HeaderValue::from_str("Mein Server").unwrap());
//     header_map
// }


// fn getItems<T: Serialize, F: Fn()->T>(cb: F)  {
//     let res = warp::reply::json(&cb());
//     ()
// }

// async fn get_items(param: GetItems)->Result<impl warp::Reply, warp::Rejection> {
//     match get_directory_items(&param.path) {
//         Ok(items ) => Ok (warp::reply::json(&items)),
//         Err(err) => {
//             println!("Could not get items: {}", err);
//             Err(warp::reject())
//         }
//     }
// }

// async fn get_exifs(param: GetExifs)->Result<impl warp::Reply, warp::Rejection> {
//     match get_exif_items(&param.path, &param.items) {
//         Ok(items ) => Ok (warp::reply::json(&items)),
//         Err(err) => {
//             println!("Could not get items: {}", err);
//             Err(warp::reject())
//         }
//     }
// }

// fn not_found() -> Response<Body> {
//     Response::builder()
//         .status(StatusCode::NOT_FOUND)
//         .body(NOTFOUND.into())
//         .unwrap()
// }

// async fn get_icon(param: GetIcon)->Result<impl warp::Reply, warp::Rejection> {
//     let path = requests::get_icon(&param.ext);

//     match tokio::fs::File::open(path.clone()).await {
//         Ok(file) => {
//             let stream = FramedRead::new(file, BytesCodec::new());
//             let body = hyper::Body::wrap_stream(stream);
//             let mut response = Response::new(body);

//             let headers = response.headers_mut();
//             let mut header_map = create_headers();
//             if let Some(ext_index) = path.rfind('.') {

//                 let ext = &path[ext_index..].to_lowercase();
//                 let content_type = match ext.as_str() {
//                     ".png" => "image/png".to_string(),
//                     ".svg" => "image/svg".to_string(),
//                     _ => "image/jpg".to_string()    
//                 };
//                 header_map.insert("Content-Type", HeaderValue::from_str(&content_type).unwrap());
//             }
//             headers.extend(header_map);
//             Ok (response)
//         },
//         Err(err) => {
//             println!("Could not get icon: {}", err);
//             Ok(not_found())
//         }
//     }
// }


//         let route_get_items = 
//             warp::post()
//             .and(warp::path("commander"))
//             .and(warp::path("getitems"))
//             .and(warp::path::end())
//             .and(warp::body::json())
//             .and_then(get_items);

//         let route_get_icon = 
//             warp::path("commander")
//             .and(warp::path("geticon"))
//             .and(warp::path::end())
//             .and(warp::query::query())
//             .and_then(get_icon);

//         let route_get_exifs = 
//             warp::post()
//             .and(warp::path("commander"))
//             .and(warp::path("getexifs"))
//             .and(warp::path::end())
//             .and(warp::body::json())
//             .and_then(get_exifs);

//             fn add_headers(reply: File)->Response<Body> {
//             let mut res = reply.into_response();
//             let headers = res.headers_mut();
//             let header_map = create_headers();
//             headers.extend(header_map);
//             res
//         }

//         let route_static = warp::fs::dir(".")
//             .map(add_headers);

//         let routes = 
//             route_get_root
//             .or(route_get_items)
//             .or(route_get_exifs)
//             .or(route_get_icon)
//             .or(route_static);
    
//         warp::serve(routes)
//             .run(([127, 0, 0, 1], port))
//             .await;        
//     });
// }

async fn get_root()->Result<impl warp::Reply, warp::Rejection> {
    match get_root_items() {
        Ok(items ) => Ok (warp::reply::json(&items)),
        Err(err) => {
            println!("Could not get root items: {}", err);
            Err(warp::reject())
        }
    }
}

pub fn server(rt: &Runtime, socket_addr: SocketAddr, static_dir: String) {
    rt.spawn(async move {

        let route_get_root = 
            warp::get()
            .and(warp::path("commander"))
            .and(warp::path("getroot"))
            .and(warp::path::end())
            .and_then(get_root);

        let route_static = dir(static_dir)
            .map(add_headers);

        let routes = route_get_root
            .or(route_static);

        warp::serve(routes)
            .run(socket_addr)
            .await;        
    });
}
