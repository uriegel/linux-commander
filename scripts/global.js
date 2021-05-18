function initializeOnTheme(onTheme) {
    onThemeCallback = onTheme
    return initialTheme
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

function showHidden(hidden) {
    console.log("showHidden", hidden)
}

var onThemeCallback
var initialTheme