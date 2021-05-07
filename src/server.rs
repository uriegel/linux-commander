extern crate chrono;
use chrono::Utc;

use warp::{Filter, Reply, fs::File, http::HeaderValue, hyper::{Body, HeaderMap, Response}};
use tokio::runtime::Runtime;

pub fn start(rt: &Runtime, port: u16)-> () {
    println!("Starting server...");

    rt.spawn(async move {
        println!("Running test server on http://localhost:{}", port); 
    
        let route1 = warp::path!("hello" / String)
            .map(|name| {
                format!("Hello, {}!", name)
            });

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

        let route2 = warp::fs::dir(".")
            .map(add_headers);

        let routes = route1.or(route2);
    
        warp::serve(routes)
            .run(([127, 0, 0, 1], port))
            .await;        
    });
}
