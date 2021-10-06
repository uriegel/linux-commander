function initializeCallbacks(onTheme, onShowHidden, onShowViewer, onRefresh) {
    onThemeCallback = onTheme
    onShowHiddenCallback = onShowHidden
    onShowViewerCallback = onShowViewer
    onRefreshCallback = onRefresh
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

function refresh(id) {
    if (onRefreshCallback)
        onRefreshCallback(id)
}

function sendMessageToWebView(command, param) {
    alert(`!!webmsg!!${command}!!${param}`)
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


