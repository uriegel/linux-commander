use std::net::{IpAddr, Ipv4Addr, SocketAddr};

use chrono::Utc;
use serde::Deserialize;
use tokio::runtime::Runtime;
use tokio_util::codec::{BytesCodec, FramedRead};
use warp::{Filter, http::HeaderValue, hyper::{self, HeaderMap}};
use warp_range::{filter_range, get_range, with_partial_content_status};

pub fn start(rt: &Runtime) {
    let socket_addr = SocketAddr::new(IpAddr::V4(Ipv4Addr::new(127, 0, 0, 1)), 9865);
    rt.spawn(async move {
        let route_get_view = 
            warp::path("commander")
            .and(warp::path("getview"))
            .and(warp::path::end())
            .and(warp::query::query())
            .and_then(get_view);
        let route_get_video = 
            warp::path("commander")
            .and(warp::path("getvideo"))
            .and(warp::path::end())
            .and(warp::query::query())
            .and_then(get_video);
        let route_get_video_range = 
            warp::path("commander")
            .and(warp::path("getvideo"))
            .and(warp::path::end())
            .and(warp::query::query())
            .and(filter_range())
            .and_then(get_video_range)
            .map(with_partial_content_status);

        let routes = route_get_view
            .or(route_get_video_range)
            .or(route_get_video); 
            
        warp::serve(routes)
            .run(socket_addr)
            .await;           
    });
}

#[derive(Deserialize)]
struct GetView {
    pub path: String
}

async fn get_view(param: GetView) -> Result<impl warp::Reply, warp::Rejection> {
    if let Some(ext_pos) = param.path.rfind(".") {
        let ext = &param.path[ext_pos+1..].to_lowercase();        
        match ext.as_str() {
            "jpg" | "png" => {
                match tokio::fs::File::open(param.path).await {
                    Ok(file) => {
                        let stream = FramedRead::new(file, BytesCodec::new());
                        let body = hyper::Body::wrap_stream(stream);
                        Ok (warp::reply::Response::new(body))
                    },
                    Err(err) => {
                        println!("Could not get img: {}", err);
                        Err(warp::reject())
                    }
                }
            },
            "pdf" => {
                match tokio::fs::File::open(param.path).await {
                    Ok(file) => {
                        let stream = FramedRead::new(file, BytesCodec::new());
                        let body = hyper::Body::wrap_stream(stream);
                        let mut response = warp::reply::Response::new(body);
                        let headers = response.headers_mut();
                        let mut header_map = create_headers();
                        header_map.insert("Content-Type", HeaderValue::from_str("application/pdf").unwrap());
                        headers.extend(header_map);
                        Ok (response)
                    },
                    Err(err) => {
                        println!("Could not get pdf: {}", err);
                        Err(warp::reject())
                    }
                }
            },
            "mp4"|"mkv" => {
                match tokio::fs::File::open(param.path).await {
                    Ok(file) => {
                        let stream = FramedRead::new(file, BytesCodec::new());
                        let body = hyper::Body::wrap_stream(stream);
                        let mut response = warp::reply::Response::new(body);
                        let headers = response.headers_mut();
                        let mut header_map = create_headers();
                        header_map.insert("Content-Type", HeaderValue::from_str("video/mp4").unwrap());
                        headers.extend(header_map);
                        Ok (response)
                    },
                    Err(err) => {
                        println!("Could not get pdf: {}", err);
                        Err(warp::reject())
                    }
                }
            },
            _ => Err(warp::reject())
        }
    } else {
        Err(warp::reject())
    }
}

fn create_headers() -> HeaderMap {
    let mut header_map = HeaderMap::new();
    let now = Utc::now();
    let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
    header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
    header_map.insert("Server", HeaderValue::from_str("webview-app").unwrap());
    header_map
}

async fn get_video(param: GetView) -> Result<impl warp::Reply, warp::Rejection> {
    get_range("".to_string(), &param.path, "video/mp4").await
}

async fn get_video_range(param: GetView, range_header: String) -> Result<impl warp::Reply, warp::Rejection> {
    get_range(range_header, &param.path, "video/mp4").await
}

