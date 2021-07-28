

        let route_get_items = 
            warp::post()
            .and(warp::path("commander"))
            .and(warp::path("getitems"))
            .and(warp::path::end())
            .and(warp::body::json())
            .and(with_events(event_sinks.clone()))
            .and_then(get_items);




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


