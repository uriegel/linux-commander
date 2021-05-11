import { formatSize } from "./renderTools.js"

export const getRoot = folderId => {
    const getColumns = () => {
        const widthstr = localStorage.getItem(`${folderId}-widths`)
        const widths = widthstr ? JSON.parse(widthstr) : []
        let columns = [{
            name: "Name",
            render: (td, item) => {
                var t = document.querySelector('#driveIcon')
                td.appendChild(document.importNode(t.content, true))
                const span = document.createElement('span')
                span.innerHTML = item.name
                td.appendChild(span)
            }            
        }, {
            name: "Mountpoint",
            render: (td, item) => td.innerHTML = item.mountPoint
        }, {
            name: "Bezeichnung",
            render: (td, item) => td.innerHTML = item.display
        }, {
            name: "Größe",
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

    const getItems = async () => {
        const response = await fetch('/commander/getroot')
        return await response.json()

    }

    const saveWidths = widths => localStorage.setItem(`${folderId}-widths`, JSON.stringify(widths))

    return {
        getColumns,
        getItems,
        saveWidths, 
    }
}