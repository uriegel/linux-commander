use crate::windows::utf_16_null_terminiated;
use std::{ mem, ptr};
use winapi::um::fileapi::GetVolumeInformationW;
use winapi::um::fileapi::GetDiskFreeSpaceExW;
use winapi::um::winnt::ULARGE_INTEGER;
use winapi::um::fileapi::GetLogicalDriveStringsW;
use crate::requests::Error;
use serde::{Serialize};

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub display: String,
    pub capacity: u64,
}

pub fn get_root_items()->Result<Vec<RootItem>, Error> {

    fn extract_drives(wchrs: &[u16])->Vec<String> {
        fn get_part(wchrs: &[u16])->Option<(String, &[u16])> {
            
            fn find_null(wchrs: &[u16])->Option<usize> { wchrs.iter().position(|n| {*n == 0}) }

            match find_null(wchrs) {
                Some(pos) => {
                    let part = &wchrs[..pos];
                    let str = String::from_utf16(part).unwrap();
                    Some((str.clone(), &wchrs[pos+1..]))
                },
                None => None
            }
        }

        fn split(result: &mut Vec<String>, wchrs: &[u16]) {
            if let Some((str, wchrs)) = get_part(wchrs) {
                if str.len() > 0 {
                    result.push(str);
                }
                split(result, wchrs);
            }
        }

        let mut result = Vec::<String>::new();
        split(&mut result, wchrs);
        return result;
    }

    unsafe {
        let size = GetLogicalDriveStringsW(0, ptr::null_mut());
        let mut buffer = vec!(0 as u16; size as usize);
        GetLogicalDriveStringsW(size, buffer.as_mut_ptr());
        let drive_strs = extract_drives(&buffer);
        let result: Vec<RootItem> = drive_strs.iter().map(|n|{
            let mut size: ULARGE_INTEGER = mem::zeroed();
            GetDiskFreeSpaceExW(utf_16_null_terminiated(n).as_ptr(), ptr::null_mut(), &mut size, ptr::null_mut());
            let size = size.QuadPart();
            
            let buffer_size = 500;
            let mut buffer = vec!(0 as u16; buffer_size as usize);
            GetVolumeInformationW(utf_16_null_terminiated(n).as_ptr(), buffer.as_mut_ptr(), buffer_size, 
                ptr::null_mut(), ptr::null_mut(), ptr::null_mut(), ptr::null_mut(), 0);
            let display = String::from_utf16(&buffer).unwrap();
            RootItem{name: n.clone(), capacity: *size, display}
        }).collect();
        Ok(result)
        // drive_strs.iter().map(|d| {
        //     let drive_type = GetDriveTypeW(utf_16_null_terminiated(d));
        // })
    }
}


pub fn check_extended_items(ext: &str)->bool {
    ext.ends_with(".png") 
    || ext.ends_with(".jpg")
    || ext.ends_with(".exe")
    || ext.ends_with(".dll")
}

pub fn get_version(path: &str)->Option<String> {

        // if let Ok(file_map) = pelite::FileMap::open(r"c:\windows\explorer.exe") {
        //     if let Ok(image) = pelite::PeFile::from_bytes(file_map.as_ref()) {
        //         if let Ok(resources) = image.resources() {
        //             if let Ok(version_info) = resources.version_info() {
        //                 let file_info = version_info.file_info();
        //                 if let Some(fixed) = file_info.fixed {
        //                     println!("Version: {:?}, {},{},{},{}", fixed.dwFileVersion, fixed.dwFileVersion.Major, fixed.dwFileVersion.Minor, fixed.dwFileVersion.Patch, fixed.dwFileVersion.Build);    
        //                 }
        //             }
        //         }
        //     }
        // }


    None
}