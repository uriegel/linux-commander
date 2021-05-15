export const DIRECTORY = "directory"
import { formatSize } from "./renderTools.js"

export const getDirectory = (folderId, path) => {
    const getType = () => DIRECTORY
    
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
            render: (td, item) => td.innerHTML = item.mountPoint
        }, {
            name: "Größe",
            isSortable: true,
            isRightAligned: true,
            render: (td, item) => {
                td.innerHTML = formatSize(item.capacity)
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

    // pub struct FileItem {
    //     name: String,
    //     time: u64,
    //     size: u64
    // }
    
    const getItems = async path => {
        const responseStr = await fetch(`/commander/getitems?path=${path}`)
        const response = await responseStr.json()
        return [{
                name: "..",
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
        getItems,
        saveWidths
    }
}