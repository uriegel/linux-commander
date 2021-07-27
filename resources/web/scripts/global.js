function initializeCallbacks(onTheme, onShowHidden, onShowViewer) {
    onThemeCallback = onTheme
    onShowHiddenCallback = onShowHidden
    onShowViewerCallback = onShowViewer
}

function initializeRequestsCallback(onRequest) {
    onRequestCallback = onRequest
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

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

const composeFunction = (...fns) => (...args) => fns.reduceRight((acc, fn) => fn(acc), args);

var onThemeCallback
var onShowHiddenCallback
var onShowViewerCallback
var onRequestCallback




