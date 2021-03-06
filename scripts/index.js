import 'grid-splitter'
import 'web-dialog-box'
import { RESULT_OK } from 'web-dialog-box'
import './components/pdfviewer.js'
import './folder.js'
import { request } from "./requests.js"
import { onShowViewer, refreshViewer} from './viewer.js'

const folderLeft = document.getElementById("folderLeft")
const folderRight = document.getElementById("folderRight")
const dialog = document.querySelector('dialog-box')

const DIRECTORY = 1
const FILE = 2
const BOTH = 3

initializeCallbacks(onTheme, onShowHidden, show => {
    onShowViewer(show, currentPath)
    folderLeft.onResize()
    folderRight.onResize()
}, refresh, onException)

function getItemsTypes(selectedItems) {
    const types = selectedItems
        .map(n => n.isDirectory)
        .filter((item, index, resultList) => resultList
            .findIndex(n => n == item) == index)
    return types.length == 1
    ? types[0] ? DIRECTORY : FILE
    : BOTH
}

;(() => {
    const initialTheme = localStorage.getItem("theme") || "themeAdwaita"
    onTheme(initialTheme)
    setInitialTheme(initialTheme) 
})()

folderLeft.addEventListener("onFocus", () => activeFolder = folderLeft)
folderRight.addEventListener("onFocus", () => activeFolder = folderRight)

const onPathChanged = evt => {
    currentPath = evt.detail.path
    refreshViewer(evt.detail.path)
    setTitle(evt.detail.path, evt.detail.dirs, evt.detail.files)
}

function refresh(folderId) {
    const folder = folderId == "folderLeft" ? folderLeft : folderRight
    folder.reloadItems()
}

function onException(text) {
    setTimeout(async () => {
        await dialog.show({
            text,
            btnOk: true
        })
        activeFolder.setFocus()        
    },
    // TODO stack MessageBoxes
    500)
}

folderLeft.addEventListener("pathChanged", onPathChanged)
folderRight.addEventListener("pathChanged", onPathChanged)
folderLeft.addEventListener("tab", () => folderRight.setFocus())
folderRight.addEventListener("tab", () => folderLeft.setFocus())
folderLeft.addEventListener("rename", evt => onRename(evt.detail))
folderRight.addEventListener("rename", evt => onRename(evt.detail))
folderLeft.addEventListener("delete", evt => onDelete(evt.detail))
folderRight.addEventListener("delete", evt => onDelete(evt.detail))
folderLeft.addEventListener("copy", evt => onCopy(evt.detail, folderRight.getCurrentPath()))
folderRight.addEventListener("copy", evt => onCopy(evt.detail, folderLeft.getCurrentPath()))
folderLeft.addEventListener("move", evt => onMove(evt.detail, folderRight.getCurrentPath()))
folderRight.addEventListener("move", evt => onMove(evt.detail, folderLeft.getCurrentPath()))
folderLeft.addEventListener("createFolder", evt => onCreateFolder(evt.detail))
folderRight.addEventListener("createFolder", evt => onCreateFolder(evt.detail))

async function onCopy(itemsToCopy, path) {
    const itemsType = getItemsTypes(itemsToCopy)
    const text = itemsType == FILE 
        ? itemsToCopy.length == 1 
            ? "M??chtest Du die Datei kopieren?"
            : "M??chtest Du die Dateien kopieren?"
        : itemsType == DIRECTORY
        ?  itemsToCopy.length == 1 
            ? "M??chtest Du den Ordner kopieren?"
            : "M??chtest Du die Ordner kopieren?"
        : "M??chtest Du die Eintr??ge kopieren?"

    const res = await dialog.show({
        text,
        btnOk: true,
        btnCancel: true
    })    
    activeFolder.setFocus()
    if (res.result == RESULT_OK)
        await request("copy", {
            id: getInactiveFolder().id,
            sourcePath: activeFolder.getCurrentPath(),
            destinationPath: path,
            directories: itemsToCopy.filter(n => n.isDirectory).map(n => n.name),
            files: itemsToCopy.filter(n => !n.isDirectory).map(n => n.name)
        })
}

async function onMove(itemsToMove, path) {
    const itemsType = getItemsTypes(itemsToMove)
    const text = itemsType == FILE 
        ? itemsToMove.length == 1 
            ? "M??chtest Du die Datei verschieben?"
            : "M??chtest Du die Dateien verschieben?"
        : itemsType == DIRECTORY
        ?  itemsToMove.length == 1 
            ? "M??chtest Du den Ordner verschieben?"
            : "M??chtest Du die Ordner verschieben?"
        : "M??chtest Du die Eintr??ge verschieben?"

    const res = await dialog.show({
        text,
        btnOk: true,
        btnCancel: true
    })    
    activeFolder.setFocus()
    if (res.result == RESULT_OK) {
        await request("move", {
            ids: [ activeFolder.id, getInactiveFolder().id ],
            sourcePath: activeFolder.getCurrentPath(),
            destinationPath: path,
            directories: itemsToMove.filter(n => n.isDirectory).map(n => n.name),
            files: itemsToMove.filter(n => !n.isDirectory).map(n => n.name)
        })
    }
}

async function onRename(itemToRename) {
    const itemsType = getItemsTypes(itemToRename)
    const text = itemsType == FILE 
        ? "Datei umbenennen"
        : "Ordner umbenennen"
    
    const getInputRange = () => {
        const pos = itemToRename[0].name.lastIndexOf(".")
        if (pos == -1)
            return [0, itemToRename[0].name.length]
        else
            return [0, pos]
    }

    const res = await dialog.show({
        text,
        input: true,
        inputText: itemToRename[0].name,
        inputSelectRange: getInputRange(),
        btnOk: true,
        btnCancel: true,
        defBtnOk: true
    })    
    activeFolder.setFocus()
    if (res.result == RESULT_OK)
        await request("rename", {
            id: activeFolder.id,
            path: activeFolder.getCurrentPath(),
            item: itemToRename[0].name,
            newName: res.input,
            isDirectory: itemsType == DIRECTORY
        })
}

async function onDelete(itemsToDelete) {
    const itemsType = getItemsTypes(itemsToDelete)
    const text = itemsType == FILE 
        ? itemsToDelete.length == 1 
            ? "M??chtest Du die Datei l??schen?"
            : "M??chtest Du die Dateien l??schen?"
        : itemsType == DIRECTORY
        ?  itemsToDelete.length == 1 
            ? "M??chtest Du den Ordner l??schen?"
            : "M??chtest Du die Ordner l??schen?"
        : "M??chtest Du die Eintr??ge l??schen?"

    const res = await dialog.show({
        text,
        btnOk: true,
        btnCancel: true
    })    
    activeFolder.setFocus()
    if (res.result == RESULT_OK)
        await request("delete", {
            id: activeFolder.id,
            sourcePath: activeFolder.getCurrentPath(),
            Items: itemsToDelete.map(n => n.name)
        })
}

async function onCreateFolder(selectedItem) {

    const res = await dialog.show({
        text: "Neuen Ordner anlegen",
        input: true,
        inputText: selectedItem.length == 1 ? selectedItem[0].name : "",
        btnOk: true,
        btnCancel: true,
        defBtnOk: true
    })    
    activeFolder.setFocus()
    if (res.result == RESULT_OK)
        await request("createFolder", {
            id: activeFolder.id,
            path: activeFolder.getCurrentPath(),
            newName: res.input
        })
}

function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => document.body.classList.remove(n))
    document.body.classList.add(theme)    
    folderLeft.changeTheme(theme)
    folderRight.changeTheme(theme)
    localStorage.setItem("theme", theme)
}

function onShowHidden(hidden) {
    folderLeft.showHidden(hidden)
    folderRight.showHidden(hidden)
}

folderLeft.setFocus()

const getInactiveFolder = () => activeFolder == folderLeft ? folderRight : folderLeft

document.addEventListener("keydown", async evt => {
    switch (evt.which) {
        case 120: { // F9
            getInactiveFolder().changePath(activeFolder.getCurrentPath())
            evt.preventDefault()
            evt.stopPropagation()
            break
        }
    }
})

var activeFolder = folderLeft
var currentPath = ""






