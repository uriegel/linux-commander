export const DIRECTORY = "directory"
import { formatDateTime, formatSize, getExtension } from "./renderTools.js"
import { ROOT } from "./root.js"

var exifColor = getComputedStyle(document.body).getPropertyValue('--exif-color') 
var selectedExifColor = getComputedStyle(document.body).getPropertyValue('--selected-exif-color') 

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
                td.innerHTML = formatDateTime(item.exifdate || item.time)
                if (item.exifdate)
                    td.style.color = item.isSelected ? selectedExifColor : exifColor
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
        let pos = path.lastIndexOf('/')
        return pos ? path.substr(0, pos) : "/"
    }

    const getPath = item => item.isDirectory 
        ? item.name != ".."
            ? currentPath + '/' + item.name 
            : currentPath != "/"
                ? getParentDir(currentPath)
                : ROOT
        : null

    const getItems = async path => {
        currentPath = path
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
        return [{
                name: "..",
                isDirectory: true,
                isNotSelectable: true
            }]
            .concat(response.dirs)
            .concat(response.files)
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
                items[n.index].exifdate = n.exifdate
            })
            return response.length ? true : false
        }
        else
            return false
    }

    const saveWidths = widths => localStorage.setItem(`${folderId}-directory-widths`, JSON.stringify(widths))

    return {
        getType,
        getColumns,
        addExtensions,
        renderRow,
        getPath,
        getItems,
        saveWidths
    }
}

// TODO exif date: inject style, set class exif
// TODO style Hidden
// TODO show Hidden
// TODO change parent: select last folder
// TODO History wich backspace and ctrl backspace
// TODO Sorting
// TODO Restricting per sort table