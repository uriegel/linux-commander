export const DIRECTORY = "directory"
import { formatDateTime, formatSize, getExtension } from "./renderTools.js"
import { ROOT } from "./root.js"

const pathDelimiter = isLinux() ? "/" : "\\"

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
        if (!isLinux() && parent.indexOf("\\") == -1)
            parent += pathDelimiter
        return [parent, path.substr(pos + 1)]
    }

    const getCurrentPath = () => currentPath

    const parentIsRoot = () => {
        if (isLinux()) {
            return currentPath == pathDelimiter
        } else 
            return currentPath.endsWith(":\\")
    }

    const getPath = item => item.isDirectory 
        ? item.name != ".."
            ? [
                isLinux() 
                ? currentPath + pathDelimiter + item.name 
                : currentPath.endsWith(":\\")
                    ? currentPath + item.name
                    : currentPath + pathDelimiter + item.name, 
                null]
            : parentIsRoot()  
                ? [ROOT, null]
                : getParentDir(currentPath)
        : [null, null]

    const getItems = async path => {
        const responseStr = await fetch("/commander/getitems", { 
            method: 'POST', 
            headers: {
                'Content-Type': 'application/json'
            },            
            body: JSON.stringify({
                path
            }) 
        })
        const response = await responseStr.json()
        let result = [{
                name: "..",
                isDirectory: true,
                isNotSelectable: true
            }]
            .concat(response.dirs)
            .concat(response.files)
        if (result && result.length)
            currentPath = path
        return result
    }    

    const addExtensions = async items => {
        const imageItems = items
            .map((n, index) => ({ index, name: n.name}))
            .filter((n, i) => n.name && (n.name.toLowerCase().endsWith(".jpg") || n.name.toLowerCase().endsWith(".png")))

        if (imageItems.length) {
            const responseStr = await fetch("/commander/getexifs", { 
                method: 'POST', 
                headers: {
                    'Content-Type': 'application/json'
                },            
                body: JSON.stringify({
                    path: currentPath,
                    items: imageItems
                }) 
            })
            const response = await responseStr.json()
            response.forEach(n => {
                items[n.index].exiftime = n.exiftime
            })
            return response.length ? true : false
        }
        else
            return false
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

    const getItem = item => isLinux() 
        ? currentPath == pathDelimiter ? pathDelimiter + item.name : currentPath + pathDelimiter + item.name
        : currentPath.endsWith(":\\") ? currentPath + item.name : currentPath + pathDelimiter + item.name

    return {
        getType,
        getColumns,
        addExtensions,
        renderRow,
        getCurrentPath,
        getPath,
        getItems,
        getSortFunction,
        saveWidths,
        getItem
    }
}
