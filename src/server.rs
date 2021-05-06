extern crate chrono;
use chrono::Utc;

use warp::{Filter, filters::reply::WithHeaders, http::HeaderValue, hyper::HeaderMap};
use tokio::runtime::Runtime;

pub fn start(rt: &Runtime, port: u16)-> () {
    println!("Starting server...");

    rt.spawn(async move {
        println!("Running test server on http://localhost:{}", port); 
    
        let route1 = warp::path!("hello" / String)
            .map(|name| {
                format!("Hello, {}!", name)
            });
    
        fn expires_now() -> WithHeaders {
            let mut headers = HeaderMap::new();
            let now = Utc::now();
            let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
            headers.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
            warp::reply::with::headers(headers)
        }

        let route2 = warp::fs::dir(".").with(expires_now());
        let routes = route1.or(route2);
    
        warp::serve(routes)
            .run(([127, 0, 0, 1], port))
            .await;        
    });
}
