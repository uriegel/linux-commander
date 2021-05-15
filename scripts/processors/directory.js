export const DIRECTORY = "directory"
import { formatDateTime, formatSize } from "./renderTools.js"
import { ROOT } from "./root.js"

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
                var t = document.querySelector('#driveIcon')
                td.appendChild(document.importNode(t.content, true))
                const span = document.createElement('span')
                span.innerHTML = item.name
                td.appendChild(span)
            }            
        }, {
            name: "Datum",
            isSortable: true,
            render: (td, item) => td.innerHTML = formatDateTime(item.time)
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

    // FileItem {
    //     name: String,
    //     isDirectory
    //     time: u64,
    //     size: u64
    // }
    const getItems = async path => {
        currentPath = path
        const responseStr = await fetch(`/commander/getitems?path=${path}`)
        const response = await responseStr.json()
        return [{
                name: "..",
                isDirectory: true,
                isNotSelectable: true
            }]
            .concat(response.dirs)
            .concat(response.files)
    }    

    const saveWidths = widths => localStorage.setItem(`${folderId}-directory-widths`, JSON.stringify(widths))

    return {
        getType,
        getColumns,
        renderRow,
        getPath,
        getItems,
        saveWidths
    }
}

// TODO icon
// TODO exif date