use block_utils::{BlockUtilsError, FilesystemType, get_block_partitions_iter, get_device_info};
use serde::{Serialize};

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub mount_point: String,
    pub capacity: u64,
    pub media_type: bool,
    pub file_system: bool
}

pub fn get_root_items()->Result<Vec<RootItem>, BlockUtilsError> {
    Ok(get_block_partitions_iter()?
        .map(|item| match get_device_info(item) {
            Ok(item) => Some(item),
            Err(_) => None
        })
        .filter(|item| item.is_some())
        .map(|item|item.unwrap())
        .map(| item | RootItem {
            name: item.name, 
            capacity: item.capacity,
            mount_point: "mount".to_string(),
            file_system: true, //item.fs_type,
            media_type: false
        })
        .collect())
        
}
