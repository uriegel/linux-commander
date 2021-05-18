function initializeCallbacks(onTheme, onShowHidden) {
    onThemeCallback = onTheme
    onShowHiddenCallback = onShowHidden
    return initialTheme
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

function showHidden(hidden) {
    if (onShowHiddenCallback)
        onShowHiddenCallback(hidden)
}

var onThemeCallback
var initialTheme