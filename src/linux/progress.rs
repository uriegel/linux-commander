use std::{cell::RefCell, rc::Rc};

use gio::{glib::{MainContext, PRIORITY_DEFAULT_IDLE, Receiver, Sender, timeout_future_seconds}, prelude::Continue};
use gtk::{Builder, DrawingArea, Revealer, prelude::{BuilderExtManual, RevealerExt}};
use webview_app::app::AppState;

use super::requests::State;

#[derive(Clone)]
pub struct Progress {
    revealer: Revealer,
    drawing_area: DrawingArea
}

impl Progress {
    pub fn new(builder: &Builder, state: &AppState)->Self {
        let revealer: Revealer = builder.object("ProgressRevealer").unwrap();
        let drawing_area: DrawingArea = builder.object("ProgressArea").unwrap();

        let (progress_sender, receiver): (Sender<f32>, Receiver<f32>) = MainContext::channel::<f32>(PRIORITY_DEFAULT_IDLE);
        let mut val = state.lock().unwrap();
        *val = Box::new(State{ progress_sender });
    
        let progress = Progress { revealer, drawing_area };
        progress.init_receiver(receiver);
        progress
    }

    fn init_receiver(&self, receiver: Receiver<f32>) {
        let progress = self.clone();
        let id_box = Rc::new(RefCell::new(0));
        
        receiver.attach( None, move |val | {
            if !progress.revealer.reveals_child() {
                progress.revealer.set_reveal_child(true);
            }
            {
                let mut id = id_box.borrow_mut();
                *id = *id + 1;
            }

            println!("Progrss: {}", val);

            if val == 1.0 {
                let main_context = MainContext::default();
                let id_clone = id_box.clone();

                let idb = id_box.borrow();
                let this_id = *idb;
                let progress_clone = progress.clone();
                main_context.spawn_local(async move {
                    timeout_future_seconds(10).await;
                    println!("Zuschlag");
                    let idb = id_clone.borrow();
                    let id = *idb;
                    if this_id == id {
                        println!("this_id: {} {}", this_id, id);
                        progress_clone.revealer.set_reveal_child(false);
                    }
                });
            }
            
            Continue(true)
        });   
    }
}


