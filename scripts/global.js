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

    alert("!!webmesg-title!!Das ist der neue Title von ShowHidden")
    if (onShowHiddenCallback)
        onShowHiddenCallback(hidden)
}

const composeFunction = (...fns) => (...args) => fns.reduceRight((acc, fn) => fn(acc), args);

var onThemeCallback
var initialTheme