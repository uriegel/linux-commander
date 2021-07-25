use std::process::Command;

fn main() {
    eprintln!("Building resources...");
    let result = Command::new("sh")
        .args(&["-c", "cd resources && glib-compile-resources resources.xml"])
        .output()
        .expect("failed to execute process");
    eprintln!("Resources built: {:?}", result);
}