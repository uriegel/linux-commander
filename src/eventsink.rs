use std::{collections::HashMap, convert::Infallible, sync::{Arc, Mutex, atomic::{AtomicUsize, Ordering}}};
use futures::StreamExt;
use tokio::{spawn, sync::mpsc};
use tokio_stream::wrappers::UnboundedReceiverStream;
use warp::{Filter, Rejection, Reply, ws::{Message, WebSocket, Ws}};

type EventSink = mpsc::UnboundedSender<Result<Message, warp::Error>>;

struct EventSession {
    event_sink: EventSink,
    request_id: AtomicUsize
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
        self.es.lock().unwrap().insert(id, EventSession { event_sink, request_id: AtomicUsize::new(0) } );    
    }

    pub fn register_request(&self, id: String) -> Option<usize> {
        if let Some(session) = self.es.lock().unwrap().get(&id) {
            Some(session.request_id.fetch_add(1, Ordering::Relaxed) + 1)
        } else {
            None
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
            let _ = session.event_sink.send(Ok(Message::text(msg)));
        }
    }
    
}

pub fn with_events(event_sinks: EventSinks) -> impl Filter<Extract = (EventSinks,), Error = Infallible> + Clone {
    warp::any()
        .map(move || event_sinks.clone() )
}

pub async fn on_eventsink(ws: Ws, id: String, event_sinks: EventSinks) -> Result< impl Reply, Rejection> {
    Ok(ws.on_upgrade(move |socket| on_connection(socket, id, event_sinks)))
}

async fn on_connection(ws: WebSocket, id: String, event_sinks: EventSinks) {
    let (client_ws_sender, _) = ws.split();
    let (client_sender, client_rcv) = mpsc::unbounded_channel();
    let client_rcv = UnboundedReceiverStream::new(client_rcv);
    spawn(client_rcv.forward(client_ws_sender));
    event_sinks.insert(id, client_sender);
}
