function isLinux() {
    if (_isLinux == undefined)
        _isLinux = location.search.endsWith("linux")
//    return _isLinux
    return true
}
function initializeCallbacks(onTheme, onShowHidden, onShowViewer) {
    onThemeCallback = onTheme
    onShowHiddenCallback = onShowHidden
    onShowViewerCallback = onShowViewer
}

function initializeFolderEvents(folderId, onEvent) {
    eventSinks.set(folderId, onEvent)
}

function initializeRequestsCallback(onRequest) {
    onRequestCallback = onRequest
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

// TODO: per request -> g_idle_add
function sendMessageToWebView(command, param) {
    alert(`!!webmsg!!${command}!!${param}`)
}

function sendRequestToWebView(command, id, param) {
    alert(`!!request!!${command}!!${id}${(param ? `!!${JSON.stringify(param)}` : "")}`)
}

function requestResult(id, result) {
    onRequestCallback(id, result)
}

function setTitle(title, dirs, files) {
    sendMessageToWebView("title", `${title} - ${dirs} Dirs, ${files} Files`)
}

function setInitialTheme(theme) {
    if (isLinux())
        sendMessageToWebView("theme", theme)
}

function showHidden(hidden) {
    if (onShowHiddenCallback)
        onShowHiddenCallback(hidden)
}

function showViewer(show) {
    if (onShowViewerCallback)
        onShowViewerCallback(show)
}

function onExifitems(folderId, exif_items) {
    let onEvent = eventSinks.get(folderId)
    onEvent(exif_items)
}

function refreshFolder(folderId) {
    let onEvent = eventSinks.get(folderId)
    onEvent({msgType: "Refresh"})
}

const composeFunction = (...fns) => (...args) => fns.reduceRight((acc, fn) => fn(acc), args);

var onThemeCallback
var onShowHiddenCallback
var onShowViewerCallback
var onRequestCallback
var eventSinks = new Map()
var _isLinux = undefined


