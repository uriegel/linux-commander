use core::fmt;

use serde::{Serialize, Deserialize};

#[derive(Debug)]
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GetItems {
    pub folder_id: String,
    pub path: String,
    #[serde(default)]
    pub hidden_included: bool
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RootItem {
    pub name: String,
    pub display: String,
    pub mount_point: String,
    pub capacity: u64,
    pub file_system: String,
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirectoryItems {
    pub files: Vec<FileItem>,
    pub dirs: Vec<DirItem>
}

pub enum FileType {
    Dir(DirItem),
    File(FileItem)
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileItem {
    pub name: String,
    pub is_hidden: bool,
    pub time: u128,
    pub size: u64
}

#[derive(Debug)]
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct DirItem {
    pub name: String,
    pub is_hidden: bool,
    pub is_directory: bool
}

#[derive(Serialize)]
pub enum MsgType {
    ExifItem
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ExifItems {
    pub msg_type: MsgType,
    pub items: Vec<ExifItem>
}

#[derive(Debug)]
#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct DeleteItems {
    pub folder_id: String,
    pub path: String,
    pub items_to_delete: Vec<String>
}

#[derive(Serialize)]
pub struct ExifItem {
    index: usize,
    #[serde(skip_serializing_if = "Option::is_none")]
    #[serde(default)]
    exiftime: Option<i64>
}

impl ExifItem {
    pub fn new(index: usize, exiftime: i64) -> Self {
        ExifItem {
            index, 
            exiftime: Some(exiftime)
        }
    }
}

#[derive(Debug)]
pub struct Error {
    pub message: String
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "({})", self.message)
    }
}

impl From<std::io::Error> for Error {
    fn from(error: std::io::Error) -> Self {
        Error {message: format!("read_dir failed: {}", error)}
    }
}
