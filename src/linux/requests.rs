 use std::{any::Any, process::Command};

use gio::{Cancellable, File, glib::Sender, traits::FileExt};
use serde::{Serialize};
use webview_app::app::AppState;

use crate::requests::{Error, ExtendedItem, IteratorExt};

pub struct State {
    pub progress_sender: Sender<f32>
}




pub fn check_extended_items(ext: &str)->bool {
    ext.ends_with(".png") 
    || ext.ends_with(".jpg")
}

pub fn get_version(_: &str, _: usize)->Option<ExtendedItem> {
    None
}

pub async fn delete(path: &str, items: Vec<String>, state: AppState) {
    
    let files_to_delete: Vec<String> = items.iter().map(|file|{
        path.to_string() + if path.ends_with("/") { "" } else { "/" } + file
    }).collect();
    
    let count = files_to_delete.len();
    for (pos, filepath) in files_to_delete.iter().enumerate() {
        let file = File::for_path(filepath);
        let result = file.trash::<Cancellable>(None);
        send_progress(&state, count, pos + 1);
    }
}

fn send_progress(state: &AppState, size: usize, val: usize) {
    let s = state.lock().unwrap();
    let r: &dyn Any = s.as_ref();
    let dc = r.downcast_ref::<State>().unwrap();
    let val = val as f32 / size as f32;
    dc.progress_sender.send(val).ok();
}
