use serde::{Serialize};
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub mount_point: String,
    pub size: u64
}

pub fn get_root_items()->Vec<RootItem> {
    vec![ RootItem { 
        mount_point: "na was".to_string(), 
        name: "dreif".to_string(),
        size: 3456u64
    }]
}
