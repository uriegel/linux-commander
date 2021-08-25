export const DIRECTORY = "directory"
import { formatDateTime, formatSize, getExtension } from "./renderTools.js"
import { ROOT } from "./root.js"
import { request } from "../requests.js"

const pathDelimiter = "/"

export const getDirectory = (folderId, path) => {
    const getType = () => DIRECTORY
    
    let currentPath = ""

    const getColumns = () => {
        const widthstr = localStorage.getItem(`${folderId}-directory-widths`)
        const widths = widthstr ? JSON.parse(widthstr) : []
        let columns = [{
            name: "Name",
            isSortable: true,
            subItem: {
                name: "Ext.",
                isSortable: true
            },            
            render: (td, item) => {
                const selector = item.name == ".." 
                    ? '#parentIcon' 
                    : item.isDirectory
                        ? '#folderIcon'
                        : '#fileIcon'
                if (selector != '#fileIcon') {
                    var t = document.querySelector(selector)
                    td.appendChild(document.importNode(t.content, true))
                } else {
                    const img = document.createElement("img")
                    const ext = getExtension(item.name)
                    if (ext) {
                        img.src = `commander/geticon?ext=${ext}`
                        img.classList.add("image")
                        td.appendChild(img)
                    } else {
                        var t = document.querySelector(selector)
                        td.appendChild(document.importNode(t.content, true))
                    }
                }

                const span = document.createElement('span')
                span.innerHTML = item.name
                td.appendChild(span)
            }            
        }, {
            name: "Datum",
            isSortable: true,
            render: (td, item) => {
                td.innerHTML = formatDateTime(item.exiftime || item.time)
                if (item.exiftime)
                    td.classList.add("exif")
            }
        }, {
            name: "Größe",
            isSortable: true,
            isRightAligned: true,
            render: (td, item) => {
                td.innerHTML = formatSize(item.size)
                td.classList.add("rightAligned")
            }
        }]
        if (widths)
            columns = columns.map((n, i)=> ({ ...n, width: widths[i]}))
        return columns
    }

    const renderRow = (item, tr) => {
        if (item.isHidden)
            tr.style.opacity = 0.5
    }

    const getParentDir = path => {
        let pos = path.lastIndexOf(pathDelimiter)
        let parent = pos ? path.substr(0, pos) : pathDelimiter
        return [parent, path.substr(pos + 1)]
    }
    
    const getCurrentPath = () => currentPath

    const parentIsRoot = () => currentPath == pathDelimiter
    
    const getPath = item => item.isDirectory 
        ? item.name != ".."
            ? [
                currentPath != "/" ? currentPath + pathDelimiter + item.name : currentPath + item.name, 
                null]
            : parentIsRoot()  
                ? [ROOT, null]
                : getParentDir(currentPath)
        : [null, null]

    const getItems = async (id, requestId, path, hiddenIncluded) => {
        var response = await request("getitems", { id, requestId, path, hiddenIncluded })
        let result = [{
                name: "..",
            isNotSelectable: true,
                isDirectory: true
            }]
            .concat(response.dirs)
            .concat(response.files)
        if (result && result.length)
            currentPath = path
        return result
    }    

    const getSortFunction = (column, isSubItem) => {
        switch (column) {
            case 0:
                return isSubItem == false 
                    ? ([a, b]) => a.name.localeCompare(b.name)
                    : ([a, b]) => getExtension(a.name).localeCompare(getExtension(b.name))
            case 1: 
                return ([a, b]) => (a.exiftime ? a.exiftime : a.time) - (b.exiftime ? b.exiftime : b.time)
            case 2: 
                return ([a, b]) => a.size - b.size
            default:
                return null
        }
    }

    const saveWidths = widths => localStorage.setItem(`${folderId}-directory-widths`, JSON.stringify(widths))

    const getItem = item => currentPath == pathDelimiter ? pathDelimiter + item.name : currentPath + pathDelimiter + item.name

    const onEvent = (items, exifItems) => {
        exifItems.forEach(n => {
            if (n.exiftime)
                items[n.index].exiftime = n.exiftime
            else
                items[n.index].version = n.version
        })
        return exifItems.length ? true : false
    }

    const sendExtendedInfos = async (folderId, reqestId, items) => {
        var exifs = items
            .map((n, i) => {
                if (n.name.endsWith(".jpg") || n.name.endsWith(".jpeg") || n.name.endsWith(".png"))
                    return Object.assign({ index: i }, n)
                else
                    return n
            })
            .filter(n => n.index != undefined)
        // TODO: send exif request
        console.log(exifs)
    }

    const getIconPath = name => currentPath + pathDelimiter + name

    return {
        getType,
        getColumns,
        renderRow,
        getCurrentPath,
        getPath,
        getItems,
        getSortFunction,
        saveWidths,
        getItem,
        onEvent,
        getIconPath,
        sendExtendedInfos
    }
}
