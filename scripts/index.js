import './components/gridsplitter.js'
import './folder.js'

const folderLeft = document.getElementById("folderLeft")
const folderRight = document.getElementById("folderRight")
const splitter = document.getElementById('splitter')
const viewerSplitter = document.getElementById('viewerSplitter')

initializeCallbacks(onTheme, onShowHidden)
;(() => {
    const initialTheme = isLinux() ? (localStorage.getItem("theme") || "themeAdwaita") : "themeWindows"
    onTheme(initialTheme)
    setInitialTheme(initialTheme) 
})()

folderLeft.addEventListener("onFocus", evt => {
     activeFolder = folderLeft
})
folderRight.addEventListener("onFocus", evt => {
     activeFolder = folderRight
})

const onPathChanged = evt => setTitle(evt.detail.title, evt.detail.dirs, evt.detail.files)

folderLeft.addEventListener("pathChanged", onPathChanged)
folderRight.addEventListener("pathChanged", onPathChanged)
folderLeft.addEventListener("tab", () => folderRight.setFocus())
folderRight.addEventListener("tab", () => folderLeft.setFocus())

function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => {
        document.body.classList.remove(n)
        splitter.classList.remove(n)
        viewerSplitter.classList.remove(n)
    })
    document.body.classList.add(theme)    
    splitter.classList.add(theme)    
    viewerSplitter.classList.add(theme)    
    folderLeft.changeTheme(theme)
    folderRight.changeTheme(theme)
    localStorage.setItem("theme", theme)
}

function onShowHidden(hidden) {
    folderLeft.showHidden(hidden)
    folderRight.showHidden(hidden)
}

folderLeft.setFocus()
if (!isLinux())
    setTimeout(() => folderLeft.setFocus(), 100)

document.addEventListener("keydown", evt => {
    switch (evt.which) {
        case 120: // F9
            const inactiveFolder = activeFolder == folderLeft ? folderRight : folderLeft
            inactiveFolder.changePath(activeFolder.getCurrentPath())
            evt.preventDefault()
            evt.stopPropagation()
            break
    }
})

var activeFolder = folderLeft







