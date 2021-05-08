use std::path::PathBuf;

use block_utils::{BlockUtilsError, FilesystemType, get_block_partitions_iter, get_device_info, get_mountpoint, is_mounted};
use serde::{Serialize, Serializer};

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

pub fn get_root_items()->Result<Vec<RootItem>, BlockUtilsError> {
    Ok(get_block_partitions_iter()?
        .map(|path| match get_device_info(path.clone()) {
            Ok(item) => {
                let (mount_point, is_mounted) = 
                    match get_mountpoint(path.clone()) {
                        Ok(mount_point) => 
                            match mount_point {
                                Some(mount_point) => (mount_point.to_string_lossy().to_string(),
                                    match is_mounted(mount_point) {
                                        Ok(is_mounted) => is_mounted,
                                        _ => false
                                    }),
                                None => (String::new(), false)
                            }
                            ,
                        _ => (String::new(), false)
                    };
                    (String::new(), false);
                Some({
                    RootItem {
                    name: item.name, 
                    capacity: item.capacity,
                    mount_point,
                    file_system: item.fs_type as i16,
                    media_type: item.media_type as i16,
                    is_mounted
                }})
            },
            Err(_) => None
        })
        .filter(|item| item.is_some())
        .map(|item|item.unwrap())
        .collect())
}
// TODO remove block_utils from toml