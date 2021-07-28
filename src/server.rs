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

#[derive(Debug)]
#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct DeleteItems {
    pub id: String,
    pub path: String,
    pub files: Vec<String>
}


        let route_get_items = 
            warp::post()
            .and(warp::path("commander"))
            .and(warp::path("getitems"))
            .and(warp::path::end())
            .and(warp::body::json())
            .and(with_events(event_sinks.clone()))
            .and_then(get_items);

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

async fn delete(param: DeleteItems)->Result<impl warp::Reply, warp::Rejection> {
    // task::spawn(  async move {
    //     requests::delete(&param.path, param.files, state).await;

    //     let progress = Refresh { msg_type: MsgType::Refresh };
    //     let json = serde_json::to_string(&progress).unwrap();
    //     event_sinks.send(param.id, json);
    // });
    Ok (warp::reply::json(&param))
}


