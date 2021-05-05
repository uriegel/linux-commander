function initializeOnTheme(onTheme) {
    onThemeCallback = onTheme
    return initialTheme
}

function setTheme(theme) {
    initialTheme = theme
    if (onThemeCallback)
        onThemeCallback(theme)
}

var onThemeCallback
var initialTheme