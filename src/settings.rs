use std::{fs, fs::File, io::ErrorKind, path::{PathBuf}};

pub fn initialize() {
    let settings = get_settings_path();
    let _ = match File::open(settings) {
        Ok(file) => file,
        Err(e) if e.kind() == ErrorKind::NotFound => {
            let settings = get_settings_path();
            let mut path = settings.clone();
            path.pop();
            fs::create_dir(path).unwrap();
            File::create(settings).unwrap()
        },
        Err(e) => panic!("Error: {:?}", e)
    };
    // let mut contents = String::new();
    // file.read_to_string(&mut contents)?;
    // assert_eq!(contents, "Hello, world!");
}

pub fn save_settings() {
    let settings = get_settings_path();
    let _ = File::open(settings).unwrap(); 
}

fn get_settings_path() -> PathBuf {
    let home_dir = dirs::home_dir().unwrap();
    [ 
        home_dir.to_str().unwrap(), 
        ".config", 
        "commander",
        "options"].iter().collect()
}