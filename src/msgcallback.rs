use webkit2gtk::{WebView, traits::WebViewExt};

pub fn connect_msg_callback<F: Fn(&str, &str)->() + 'static, R: Fn(&str, &str, &str)->() + 'static>(webview: &WebView, on_msg: F, on_request: R) {
    let webmsg = "!!webmsg!!";
    let request = "!!request!!";

    webview.connect_script_dialog(move|_, dialog | {
        let str = dialog.get_message();
        if str.starts_with(webmsg) {
            let msg = &str[webmsg.len()..];
            if let Some(pos) = msg.find("!!") {
                let cmd = &msg[0..pos];
                let payload = &msg[pos+2..];
                on_msg(cmd, payload);
            }
        } else if str.starts_with(request) {
            let msg = &str[request.len()..];
            if let Some(pos) = msg.find("!!") {
                let cmd = &msg[0..pos];
                let part = &msg[pos+2..];
                if let Some(pos) = part.find("!!") {
                    let id = &part[0..pos];
                    let param = &part[pos+2..];
                    on_request(cmd, id, param);
                } else {
                    on_request(cmd, part, "{}");
                }
            }
        }

        true
    });
}

