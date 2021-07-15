 use std::process::Command;

use serde::{Serialize};

use crate::{requests::{Error, ExtendedItem, IteratorExt}, server::DeleteItems};

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub display: String,
    pub mount_point: String,
    pub capacity: u64,
    pub file_system: String,
}

pub fn get_root_items()->Result<Vec<RootItem>, Error> {
    let output = Command::new("lsblk")
        .arg("--bytes")
        .arg("--output")
        .arg("SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE")
        .output().map_err(|e| Error{message: e.to_string()})?;
    if output.status.success() {
        let lines = String::from_utf8(output.stdout)
            .map_err(|e| Error{message: e.to_string()})?;

        let lines: Vec<&str> = lines.lines().collect();

        let first_line = lines[0];

        let get_part = |key: &str| {
            match first_line.match_indices(key).next() {
                Some((index, _)) => index as u16,
                None => 0
            }
        };

        let column_positions = [
            0u16, 
            get_part("NAME"),
            get_part("LABEL"),
            get_part("MOUNT"),
            get_part("FSTYPE")
        ];

        let get_string = |line: &str, pos1, pos2| {
            let index = column_positions[pos1] as usize;
            let len = match pos2 {
                | Some(pos) => Some(column_positions[pos] as usize - index),
                | None => None
            }; 
            let result: String = line
                .chars()
                .into_iter()
                .skip(index)
                .take_option(len)
                .collect();
            result
                .trim()
                .to_string()
        };

        let mut items: Vec<RootItem>= lines
            .iter()
            .skip(1)
            .map(|n| {
                let name = get_string(n, 1, Some(2));
                match name.bytes().next() {
                    Some(b) if b > 127 => {
                        let display = get_string(n, 2, Some(3));
                        let mount_point = get_string(n, 3, Some(4));
                        let capacity = match str::parse::<u64>(&get_string(n, 0, Some(1))) {
                            Ok(val) => val,
                            _ => 0
                        };
                        let file_system = get_string(n, 4, None);
                        Some(RootItem { name: name[6..].to_string(), display, mount_point, capacity, file_system })
                    },
                    _ => None
                }
            })
            .filter(|item| item.is_some())
            .map(|item|item.unwrap())
            .collect();
        let mut result = Vec::<RootItem>::new();
        if let Some(home) = dirs::home_dir() { 
            let home_item = RootItem {
                name: "~".to_string(),
                display: "".to_string(),
                mount_point: home.to_str().unwrap().to_string(),
                capacity: 0,
                file_system: "".to_string()
            };
            result.push(home_item);
        }
        result.append(&mut items);
        Ok(result)
    }
    else { 
        Err(Error {message: "Execution of lsblk failed".to_string()}) 
    }
}

pub fn is_hidden(_: &str, name: &str)->bool {
    name.as_bytes()[0] == b'.' && name.as_bytes()[1] != b'.'
}

pub fn check_extended_items(ext: &str)->bool {
    ext.ends_with(".png") 
    || ext.ends_with(".jpg")
}

pub fn get_version(_: &str, _: usize)->Option<ExtendedItem> {
    None
}

pub async fn delete(path: &str, items: Vec<String>) {
    // TODO add delete job
    tokio::time::sleep(std::time::Duration::from_secs(7)).await;
    println!("In linux: {}", path);
}