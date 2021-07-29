
// TODO Only in certain processors, not in root!
import * as requests from "./requests.js"

const testsend = async (payload, id) => new Promise((res, rej) => {
    globres.set(id, res)
    sendMessageToWebView("test", payload)
})

export const addDeleteJob = async (id, path, itemsToDelete) => {
    requests.deleteItems(id, path, itemsToDelete)
}