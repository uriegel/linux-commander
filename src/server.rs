use std::net::SocketAddr;

use serde::{Deserialize};
use tokio::runtime::Runtime;
use warp::{Filter, fs::dir};
use webview_app::headers::add_headers;
use crate::{eventsink::{EventSinks, on_eventsink, with_events}, requests::retrieve_extended_items};

#[cfg(target_os = "linux")]
use crate::linux::requests::get_root_items;
use crate::{requests::{get_directory_items, get_icon}};
#[cfg(target_os = "windows")]
use crate::windows::requests::get_root_items;

#[derive(Deserialize)]
struct GetItems {
    id: String,
    path: String,
}

pub fn server(rt: &Runtime, socket_addr: SocketAddr, static_dir: String) {
    rt.spawn(async move {

        let event_sinks = EventSinks::new();

        let route_static = dir(static_dir)
            .map(add_headers);

        let route_get_root = 
            warp::get()
            .and(warp::path("commander"))
            .and(warp::path("getroot"))
            .and(warp::path::end())
            .and_then(get_root);

        let route_get_items = 
            warp::post()
            .and(warp::path("commander"))
            .and(warp::path("getitems"))
            .and(warp::path::end())
            .and(warp::body::json())
            .and(with_events(event_sinks.clone()))
            .and_then(get_items);

        let route_get_icon = 
            warp::path("commander")
            .and(warp::path("geticon"))
            .and(warp::path::end())
            .and(warp::query::query())
            .and_then(get_icon);

        let route_events = 
            warp::path("events")
            .and(warp::ws())
            .and(warp::path::param())
            .and(with_events(event_sinks.clone()))
            .and_then(on_eventsink);
    
        let routes = route_get_items
            .or(route_get_root)
            .or(route_get_icon)
            .or(route_events)
            .or(route_static);

        warp::serve(routes)
            .run(socket_addr)
            .await;        
    });
}

async fn get_root()->Result<impl warp::Reply, warp::Rejection> {
    match get_root_items() {
        Ok(items ) => Ok (warp::reply::json(&items)),
        Err(err) => {
            println!("Could not get root items: {}", err);
            Err(warp::reject())
        }
    }
}

async fn get_items(param: GetItems, event_sinks: EventSinks)->Result<impl warp::Reply, warp::Rejection> {
    match get_directory_items(&param.path, ) {
        Ok(items ) => {
            retrieve_extended_items(param.id, param.path, &items, event_sinks);
            Ok (warp::reply::json(&items))
        },
        Err(err) => {
            println!("Could not get items: {}", err);
            Err(warp::reject())
        }
    }
}



