use std::{collections::HashMap, sync::{Arc, Mutex, atomic::{AtomicUsize, Ordering}}};

#[derive(Clone)]
pub struct Requests {
    es: Arc<Mutex<HashMap<String, AtomicUsize>>>
} 

impl Requests {
    pub fn new() -> Self {
        Self { es: Arc::new(Mutex::new(HashMap::new())) }
    }

    pub fn insert(&self, id: String) {
        self.es.lock().unwrap().insert(id, AtomicUsize::new(0));    
    }

    pub fn register_request(&self, id: &str) -> Option<usize> {
        if let Some(request_id) = self.es.lock().unwrap().get(id) {
            Some(request_id.fetch_add(1, Ordering::Relaxed) + 1)
        } else {
            None
        }
    }

    pub fn is_request_active(&self, id: &str, request_id: usize) -> bool {
        if let Some(recent_request_id) = self.es.lock().unwrap().get(id) {
            recent_request_id.load(Ordering::Relaxed) == request_id
        } else {
            false
        }
    }
}


//  use std::{any::Any, process::Command};

// use gio::{Cancellable, File, glib::Sender, traits::FileExt};
// use serde::{Serialize};
// use webview_app::app::AppState;

// use crate::requests::{Error, ExtendedItem, IteratorExt};

// pub struct State {
//     pub progress_sender: Sender<f32>
// }





// pub async fn delete(path: &str, items: Vec<String>, state: AppState) {
    
//     let files_to_delete: Vec<String> = items.iter().map(|file|{
//         path.to_string() + if path.ends_with("/") { "" } else { "/" } + file
//     }).collect();
    
//     let count = files_to_delete.len();
//     for (pos, filepath) in files_to_delete.iter().enumerate() {
//         let file = File::for_path(filepath);
//         let result = file.trash::<Cancellable>(None);
//         send_progress(&state, count, pos + 1);
//     }
// }

// fn send_progress(state: &AppState, size: usize, val: usize) {
//     let s = state.lock().unwrap();
//     let r: &dyn Any = s.as_ref();
//     let dc = r.downcast_ref::<State>().unwrap();
//     let val = val as f32 / size as f32;
//     dc.progress_sender.send(val).ok();
// }
