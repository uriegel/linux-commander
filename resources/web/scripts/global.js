function initializeCallbacks(onTheme, onShowHidden, onShowViewer) {
    onThemeCallback = onTheme
    onShowHiddenCallback = onShowHidden
    onShowViewerCallback = onShowViewer
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

function sendMessageToWebView(command, param) {
    alert(`!!webmsg!!${command}!!${param}`)
}

function sendRequestToWebView(command, param, id) {
    alert(`!!request!!${command}!!${id}!!${param}`)
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





var globres = new Map()

const endtest = ok => {
    res = globres.get(ok.id)
    globres.delete(ok.id)
    res(ok)
}
