export async function getRoot() {
    let id = ++requestIdFactory
    sendRequestToWebView("getRoot", id)
}

var requestIdFactory = 0