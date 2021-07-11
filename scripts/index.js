import './components/gridsplitter.js'
import './components/pdfviewer.js'
import './components/DialogBoxComponent.js'
import './folder.js'
import {onTheme as onViewerTheme, onShowViewer, refreshViewer} from './viewer.js'

const folderLeft = document.getElementById("folderLeft")
const folderRight = document.getElementById("folderRight")
const splitter = document.getElementById('splitter')
const dialog = document.querySelector('dialog-box-component')

initializeCallbacks(onTheme, onShowHidden, show => {
    onShowViewer(show, currentPath)
    folderLeft.onResize()
    folderRight.onResize()
})

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

const onPathChanged = evt => {
    currentPath = evt.detail.path
    refreshViewer(evt.detail.path)
    setTitle(evt.detail.path, evt.detail.dirs, evt.detail.files)
}

folderLeft.addEventListener("pathChanged", onPathChanged)
folderRight.addEventListener("pathChanged", onPathChanged)
folderLeft.addEventListener("tab", () => folderRight.setFocus())
folderRight.addEventListener("tab", () => folderLeft.setFocus())
folderLeft.addEventListener("delete", evt => onDelete(evt.detail))
folderRight.addEventListener("delete", evt => onDelete(evt.detail))

async function onDelete(itemsToDelete) {
    console.log("del", itemsToDelete)

    const res = await dialog.show({
        text: "Löschen",
        btnOk: true,
        btnCancel: true
    })    
    activeFolder.setFocus()
}

function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => {
        document.body.classList.remove(n)
        splitter.classList.remove(n)
        dialog.classList.remove(n) 
    })
    document.body.classList.add(theme)    
    splitter.classList.add(theme)    
    folderLeft.changeTheme(theme)
    folderRight.changeTheme(theme)
    dialog.classList.add(theme)        
    onViewerTheme(theme)
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
        case 114: // F3
            showViewer()
            evt.preventDefault()
            evt.stopPropagation()
            break
        case 120: // F9
            const inactiveFolder = activeFolder == folderLeft ? folderRight : folderLeft
            inactiveFolder.changePath(activeFolder.getCurrentPath())
            evt.preventDefault()
            evt.stopPropagation()
            break
    }
})

var activeFolder = folderLeft
var currentPath = ""






