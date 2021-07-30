use std::{collections::HashMap, sync::{Arc, Mutex, atomic::{AtomicUsize, Ordering}}};

#[derive(Clone)]
pub struct ActiveRequests {
    es: Arc<Mutex<HashMap<String, AtomicUsize>>>
} 

impl ActiveRequests {
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
