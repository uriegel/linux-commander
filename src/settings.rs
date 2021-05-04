use serde::{Deserialize, Serialize};
use serde_json::Result;
use std::{fs, fs::File, io::ErrorKind, path::{PathBuf}};

#[derive(Serialize, Deserialize)]
pub struct Settings {
    pub width: u32,
    pub height: u32,
}

pub fn initialize() -> Settings {
    let settings = get_settings_path();
    match File::open(settings) {
        Ok(file) => {
            Settings { width: 0, height: 0 }
        }
        Err(e) if e.kind() == ErrorKind::NotFound => {
            let settings = get_settings_path();
            let mut path = settings.clone();
            path.pop();
            fs::create_dir(path).unwrap();
            File::create(settings).unwrap();
            Settings { width: 0, height: 0 }
        },
        Err(e) => panic!("Error: {:?}", e)
    }
    // let mut contents = String::new();
    // file.read_to_string(&mut contents)?;
    // assert_eq!(contents, "Hello, world!");
}

pub fn save_settings(settings: &Settings) {
    let settings_path = get_settings_path();
    let _ = File::open(settings_path).unwrap(); 
    println!("Size {}", settings.width);
}

fn get_settings_path() -> PathBuf {
    let home_dir = dirs::home_dir().unwrap();
    [ 
        home_dir.to_str().unwrap(), 
        ".config", 
        "commander",
        "options"].iter().collect()
}