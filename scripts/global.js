
function isLinux() {
    if (_isLinux == undefined)
        _isLinux = location.search.endsWith("linux")
    return _isLinux
}

function initializeCallbacks(onTheme, onShowHidden) {
    onThemeCallback = onTheme
    onShowHiddenCallback = onShowHidden
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

function setTitle(title) {
    if (isLinux())
        sendMessageToWebView("title", title)
}

function setInitialTheme(theme) {
    if (isLinux())
        sendMessageToWebView("theme", theme)
}

function showHidden(hidden) {
    if (onShowHiddenCallback)
        onShowHiddenCallback(hidden)
}

const composeFunction = (...fns) => (...args) => fns.reduceRight((acc, fn) => fn(acc), args);

var onThemeCallback
var onShowHiddenCallback
var _isLinux = undefined
