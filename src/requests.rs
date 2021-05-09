use core::fmt;
use std::process::Command;

use serde::{Serialize};

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub mount_point: String,
    pub capacity: u64,
    pub media_type: i16,
    pub file_system: i16,
    pub is_mounted: bool
}

pub struct Error {
    pub message: String
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "({})", self.message)
    }
}

pub fn get_root_items()->Result<Vec<RootItem>, Error> {
    let output = Command::new("lsblk")
        .arg("--bytes")
        .arg("--output")
        .arg("SIZE,NAME,LABEL,MOUNTPOINT,FSTYPE")
        .output().map_err(|e| Error{message: e.to_string()})?;
    if !output.status.success() {
        Err(Error {message: "Execution of lsblk failed".to_string()})
    }
    else {
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

        column_positions
            .iter()
            .for_each(|x| println!("{}", x));

        let get_string = |line: &str, pos1, pos2| {
            line[column_positions[pos1] as usize..column_positions[pos2] as usize]
                .to_string()
        };

        let affe = get_string(lines[9], 2, 3);
        println!("Eins {}", lines[9]);
        println!("2 {}", column_positions[2]);
        println!("3 {}", lines[9][5 as usize..].to_string());

            //driveString.substring(column_positions[pos1], column_positions[pos2]).trim()            
            //driveString.substring(column_positions[pos1], column_positions[pos2]).trim()            
        
        //.for_each(|x| println!("{}", x));
        Err(Error {message: "Execution of lsblk failed".to_string()})
    }
}
