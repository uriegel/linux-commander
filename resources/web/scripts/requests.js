export function initializeRequests() {
    initializeRequestsCallback(onRequest)
}

export async function getRoot() {
    return request("getRoot")
}

export async function getItems(folderId, path, hiddenIncluded) {
    return request("getItems", { folderId, path, hiddenIncluded })
}

export async function deleteItems(folderId, path, itemsToDelete) {
    return request("deleteItems", { folderId, path, itemsToDelete })
}

async function request(method, param) {
    return new Promise((res, rej) => {
        let id = ++requestIdFactory
        requests.set(id, res)
        sendRequestToWebView(method, id, param)
    })
}

function onRequest(id, result) {
    console.log("requestResult here", id, result);
    let res = requests.get(id)
    requests.delete(id)
    res(result)
}

var requestIdFactory = 0
var requests = new Map()

