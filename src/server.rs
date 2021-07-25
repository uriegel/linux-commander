use serde::{Serialize, Deserialize};
use tokio::{runtime::Runtime, task};
use warp::{Filter, fs::dir};
use warp_range::{filter_range, with_partial_content_status};
use webview_app::{app::{AppState, WarpInitData}, headers::add_headers};
use crate::{eventsink::{
        EventSinks, on_eventsink, with_events
    }, linux::requests, requests::{MsgType, get_video, get_video_range, get_view, retrieve_extended_items}};
use crate::{requests::{get_directory_items, get_icon}};

#[cfg(target_os = "linux")]
use crate::linux::requests::get_root_items;
#[cfg(target_os = "windows")]
use crate::windows::requests::get_root_items;

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct GetItems {
    id: String,
    path: String,
    #[serde(default)]
    hidden_included: bool
}

#[derive(Debug)]
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct DeleteItems {
    pub id: String,
    pub path: String,
    pub files: Vec<String>
}

pub fn server(rt: &Runtime, data: WarpInitData) {
    let state = data.state.clone();
    rt.spawn(async move {

        let event_sinks = EventSinks::new();

        let route_static = dir(data.static_dir)
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

        let route_get_view = 
            warp::path("commander")
            .and(warp::path("getview"))
            .and(warp::path::end())
            .and(warp::query::query())
            .and_then(get_view);

        let route_delete = 
            warp::post()
            .and(warp::path("commander"))
            .and(warp::path("delete"))
            .and(warp::path::end())
            .and(warp::body::json())
            .and(with_events(event_sinks.clone()))
            .and_then(move |p, e| { delete(p, e, state.clone())});

        let route_events = 
            warp::path("events")
            .and(warp::ws())
            .and(warp::path::param())
            .and(with_events(event_sinks.clone()))
            .and_then(on_eventsink);
    
        let routes = route_get_items
            .or(route_get_root)
            .or(route_get_icon)
            .or(route_get_view)
            .or(route_get_video_range)
            .or(route_get_video)
            .or(route_delete)
            .or(route_events)
            .or(route_static);

        warp::serve(routes)
            .run(data.socket_addr)
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
    match get_directory_items(&param.path, &param.id, !param.hidden_included, event_sinks.clone()) {
        Ok(items ) => {
            retrieve_extended_items(param.id, param.path, &items, event_sinks);
            Ok (warp::reply::json(&items))
        },
        Err(err) => {
            println!("Could not get items, path: {} {}", param.path, err);
            Err(warp::reject())
        }
    }
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct Refresh {
    msg_type: MsgType,
}


async fn delete(param: DeleteItems, event_sinks: EventSinks, state: AppState)->Result<impl warp::Reply, warp::Rejection> {
    task::spawn(  async move {
        requests::delete(&param.path, param.files, state).await;

        let progress = Refresh { msg_type: MsgType::Refresh };
        let json = serde_json::to_string(&progress).unwrap();
        event_sinks.send(param.id, json);
    });
    Ok (warp::reply::reply())
}


