use serde::{Deserialize, Serialize};
use std::{fs, fs::{File}, io::{ErrorKind, Read, Write}, path::{PathBuf}};

#[derive(Serialize, Deserialize)]
pub struct Settings {
    pub width: i32,
    pub height: i32,
    #[serde(default="get_default_theme")]
    pub theme: String,
}

fn get_default_theme()->String {
    "themeAdwaitaDark".to_string()
}

pub fn initialize() -> Settings {
    let settings = get_settings_path();
    println!("{:?}",settings);
    match File::open(settings) {
        Ok(mut file) => {   
            let mut contents = String::new();
            file.read_to_string(&mut contents).unwrap();
            let settings: Settings = serde_json::from_str(&contents).unwrap();
            settings
        }
        Err(e) if e.kind() == ErrorKind::NotFound => {
            let settings = get_settings_path();
            let mut path = settings.clone();
            path.pop();
            if fs::metadata(path.clone()).is_ok() == false {
                fs::create_dir(path).unwrap();
            }
            File::create(settings).unwrap();
            Settings { width: 0, height: 0, theme: get_default_theme() }
        },
        Err(e) => panic!("Error: {:?}", e)
    }
}

// TODO: save theme

pub fn save_settings(settings: &Settings) {
    let settings_path = get_settings_path();
    let json = serde_json::to_string(&settings).unwrap();
    let mut file = File::create(settings_path).unwrap();
    file.write(&json.into_bytes()).expect("Unable to write settings");
}

fn get_settings_path() -> PathBuf {
    let home_dir = dirs::home_dir().unwrap();
    [ 
        home_dir.to_str().unwrap(), 
        ".config", 
        "commander",
        "options"].iter().collect()
}