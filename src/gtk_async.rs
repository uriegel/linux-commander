use std::{
    future::Future, pin::Pin, sync::{
        Arc, Mutex
    }, task::{
        Context, Poll, Waker
    }, thread
};

pub struct GtkFuture<T> {
    state: Arc<Mutex<State<T>>>,
}

struct State<T> {
    completed: bool,
    erg: Arc<Mutex<Option<T>>>,
    waker: Option<Waker>,
}

impl<T> Future for GtkFuture<T> {
    type Output = T;
    fn poll(self: Pin<&mut Self>, cx: &mut Context<'_>)->Poll<Self::Output> {
        let mut state = self.state.lock().unwrap();
        if state.completed {
            let data = state.erg.lock().unwrap().take().expect("No data");
            Poll::Ready(data)
        } else {
            state.waker = Some(cx.waker().clone());
            Poll::Pending
        }
    }
}

impl<T: 'static + Send> GtkFuture<T> {
    pub fn new<R: FnOnce()->T + Send + 'static>(on_request: R) -> Self {
        let state = Arc::new(Mutex::new(State {
            completed: false,
            erg: Arc::new(Mutex::new(None)),
            waker: None,
        }));

        let thread_shared_state = state.clone();
        thread::spawn(move || {
            let data = on_request();
            let mut state = thread_shared_state.lock().unwrap();
            state.erg = Arc::new(Mutex::new(Some(data)));
            state.completed = true;
            if let Some(waker) = state.waker.take() {
                waker.wake()
            }
        });

        GtkFuture { state }
    }
}
