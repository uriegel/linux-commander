function initializeOnTheme(onTheme) {
    onThemeCallback = onTheme
}

function setTheme(theme) {
    if (onThemeCallback)
        onThemeCallback(theme)
}

var onThemeCallback