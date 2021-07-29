use std::{cell::RefCell, cmp::{max, min}, f64::consts::PI, rc::Rc};

use gio::{glib::{MainContext, PRIORITY_DEFAULT_IDLE, Receiver, Sender, timeout_future_seconds}, prelude::*};
use gtk::{Builder, DrawingArea, Inhibit, Revealer, cairo::{Antialias, Context, LineCap, LineJoin}, prelude::{BuilderExtManual, RevealerExt, WidgetExt}};

#[derive(Clone)]
pub struct Progress {
    pub sender: Sender<f32>,
    revealer: Revealer,
    drawing_area: DrawingArea,
    progress: Rc<RefCell<u32>> // 1..1000
}

impl Progress {
    pub fn new(builder: &Builder)->Self {
        let revealer: Revealer = builder.object("ProgressRevealer").unwrap();
        let drawing_area: DrawingArea = builder.object("ProgressArea").unwrap();

        let (sender, receiver): (Sender<f32>, Receiver<f32>) = MainContext::channel::<f32>(PRIORITY_DEFAULT_IDLE);
    
        let drawing_area_clone = drawing_area.clone();
        let progress = Progress { revealer, drawing_area, progress: Rc::new(RefCell::new(0)), sender };
        progress.init_receiver(receiver);
        let progress_clone = progress.clone();
        drawing_area_clone.connect_draw(move|_, context| { progress_clone.draw(context) });
        progress
    }

    fn init_receiver(&self, receiver: Receiver<f32>) {
        let progress = self.clone();
        let id_box = Rc::new(RefCell::new(0));
        
        receiver.attach( None, move |val | {
            if !progress.revealer.reveals_child() {
                progress.revealer.set_reveal_child(true);
            }

            progress.set_progress(val);

            {
                let mut id = id_box.borrow_mut();
                *id = *id + 1;
            }

            if val == 1.0 {
                let main_context = MainContext::default();
                let id_clone = id_box.clone();

                let idb = id_box.borrow();
                let this_id = *idb;
                let progress_clone = progress.clone();
                main_context.spawn_local(async move {
                    timeout_future_seconds(10).await;
                    let idb = id_clone.borrow();
                    let id = *idb;
                    if this_id == id {
                        progress_clone.revealer.set_reveal_child(false);
                    }
                });
            }
            
            Continue(true)
        });   
    }

    fn draw(&self, context: &Context)->Inhibit {
        let width = self.drawing_area.allocated_width();
        let height = self.drawing_area.allocated_height();
        context.set_antialias(Antialias::Best);
        context.set_line_join(LineJoin::Miter);
        context.set_line_cap(LineCap::Round);

        context.translate(width as f64/2.0, height as f64 /2.0);
        context.stroke_preserve().ok();

        let idb = self.progress.borrow();
        let progress = *idb;
        let angle = - PI / 2.0 + (progress as f64/ 500.0) * PI;

        context.arc_negative(0.0, 0.0, 
            ((if width < height { width } else { height}) / 2 - 0).into(),
            - PI / 2.0, angle);
        context.line_to(0.0, 0.0);
        context.set_source_rgb(0.7, 0.7, 0.7);
        context.fill().ok();

        context.move_to(0.0, 0.0);
        context.arc(0.0, 0.0, 
            ((if width < height { width } else { height}) / 2 - 0).into(),
            - PI / 2.0, angle);

        context.set_source_rgb(0.3, 0.3, 0.3);
        context.fill().ok();
        Inhibit(false)
    }

    fn set_progress(&self, val: f32) {
        let val = (val * 1000.0) as u32;
        let val = max(min(val, 1000), 1);
        let mut progress = self.progress.borrow_mut();
        *progress = val;
        self.drawing_area.queue_draw();        
    }
}


