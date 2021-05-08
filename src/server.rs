extern crate chrono;
use chrono::Utc;

use warp::{Filter, Reply, fs::File, http::HeaderValue, hyper::{Body, HeaderMap, Response}};
use tokio::runtime::Runtime;

async fn get_root()->Result<impl warp::Reply, warp::Rejection> {
    let our_ids = vec![1, 3, 7, 144];
    Ok(warp::reply::json(&our_ids))
}

async fn get_items(path: String)->Result<impl warp::Reply, warp::Rejection> {
    let our_ids = vec![1, 3, 7, 77];
    Ok(warp::reply::json(&our_ids))
}

pub fn start(rt: &Runtime, port: u16)-> () {
    println!("Starting server...");

    rt.spawn(async move {
        println!("Running test server on http://localhost:{}", port); 

        let route_get_root = warp::get()
            .and(warp::path("commander"))
            .and(warp::path("getroot"))
            .and(warp::path::end())
            .and_then(get_root);

        let route_get_items = warp::get()
            .and(warp::path("commander"))
            .and(warp::path("getitems"))
            .and(warp::path::param())
            .and_then(get_items);

        fn add_headers(reply: File)->Response<Body> {
            let mut header_map = HeaderMap::new();
            let now = Utc::now();
            let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
            header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
            header_map.insert("Server", HeaderValue::from_str("Mein Server").unwrap());

            let mut res = reply.into_response();
            let headers = res.headers_mut();
            headers.extend(header_map);
            res
        }

        let route_static = warp::fs::dir(".")
            .map(add_headers);

        let routes = 
            route_get_root
            .or(route_get_items)
            .or(route_static);
    
        warp::serve(routes)
            .run(([127, 0, 0, 1], port))
            .await;        
    });
}
