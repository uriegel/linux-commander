use std::{collections::HashMap, sync::{Arc, Mutex, atomic::{AtomicBool, AtomicUsize, Ordering}}};

use glib::{Continue, MainContext, PRIORITY_DEFAULT, Sender, clone};
use gtk::glib;
use serde::{Serialize};
use webkit2gtk::WebView;

// use std::{collections::HashMap, convert::Infallible, sync::{Arc, Mutex, atomic::{AtomicBool, AtomicUsize, Ordering}}};
// use futures::StreamExt;
// use tokio::{spawn, sync::mpsc};
// use tokio_stream::wrappers::UnboundedReceiverStream;
// use warp::{Filter, Rejection, Reply, ws::{Message, WebSocket, Ws}};

#[derive(Serialize)]
pub enum MsgType {
    ExifItem,
    Refresh
}

type EventSink = Sender<String>;

struct EventSession {
    event_sink: EventSink,
    request_id: AtomicUsize,
    request_active: AtomicBool
}

#[derive(Clone)]
pub struct EventSinks {
    es: Arc<Mutex<HashMap<String, EventSession>>>
} 

impl EventSinks {
    pub fn new() -> Self {
        Self { es: Arc::new(Mutex::new(HashMap::new())) }
    }

    pub fn insert(&self, id: String, event_sink: EventSink) {
        self.es.lock().unwrap().insert(id, EventSession { 
            event_sink, 
            request_id: AtomicUsize::new(0), 
            request_active: AtomicBool::new(false)
        });    
    }

    pub fn register_request(&self, id: String) -> Option<usize> {
        if let Some(session) = self.es.lock().unwrap().get(&id) {
            Some(session.request_id.fetch_add(1, Ordering::Relaxed) + 1)
        } else {
            None
        }
    }

    pub fn set_request(&self, id: &str, active: bool) {
        if let Some(session) = self.es.lock().unwrap().get(id) {
            session.request_active.swap(active, Ordering::Relaxed);
        }
    }
    
    pub fn is_request_active(&self, id: String, request_id: usize) -> bool {
        if let Some(session) = self.es.lock().unwrap().get(&id) {
            session.request_id.load(Ordering::Relaxed) == request_id
        } else {
            false
        }
    }

    pub fn send(&self, id: String, msg: String) {
        if let Some(session) = self.es.lock().unwrap().get(&id) {
            let _ = session.event_sink.send(msg);
        }
    }
    
    pub fn active_requests(&self) -> bool {
        self.es.lock().unwrap().iter().any(|(_, s)|s.request_active.load(Ordering::Relaxed))
    }
}

pub fn initialize(event_sinks: EventSinks, webview: WebView) {
    let (sender, receiver) = MainContext::channel::<String>(PRIORITY_DEFAULT);
    event_sinks.insert("left".to_string(), sender);
    receiver.attach(None, clone!(@weak webview => @default-return Continue(true), move|msg|{
        //webview.
        Continue(true)
    }));
    let (sender, receiver) = MainContext::channel::<String>(PRIORITY_DEFAULT);
    event_sinks.insert("right".to_string(), sender);
    receiver.attach(None, clone!(@weak webview => @default-return Continue(true), move|msg|{
        //webview.
        Continue(true)
    }));
}

