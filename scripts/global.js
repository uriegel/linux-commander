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

function setTitle(title) {
    alert("!!webmesg-title!!" + title)
}

function setInitialTheme(theme) {
    alert("!!webmesg-theme!!" + theme)
}

function showHidden(hidden) {
    if (onShowHiddenCallback)
        onShowHiddenCallback(hidden)
}

const composeFunction = (...fns) => (...args) => fns.reduceRight((acc, fn) => fn(acc), args);

var onThemeCallback
var initialTheme

