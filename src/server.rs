extern crate tokio;

pub mod server {
  
    use warp::Filter;
    use tokio::runtime::Runtime;
    
    pub fn start(rt: &Runtime, port: u16)-> () {
        println!("Starting server...");

        rt.spawn(async move {
            println!("Running test server on http://localhost:{}", port); 
        
            let route1 = warp::path!("hello" / String)
                .map(|name| {
                    format!("Hello, {}!", name)
                });
        
            let route2 = warp::fs::dir("/home/uwe/Projekte/VirtualTableComponent");
            let routes = route1.or(route2);
        
            warp::serve(routes)
                .run(([127, 0, 0, 1], port))
                .await;        
        });
    }
}