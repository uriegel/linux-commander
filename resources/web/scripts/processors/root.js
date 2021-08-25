import { formatSize } from "./renderTools.js"
import { request } from "../requests.js"

export const ROOT = "root"

export const getRoot = folderId => {
    const getType = () => ROOT

    const getColumns = () => {
        const widthstr = localStorage.getItem(`${folderId}-root-widths`)
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

    const renderRow = (item, tr) => {
        if (!item.mountPoint)
            tr.style.opacity = 0.5
    }

    const getCurrentPath = () => ROOT

    const getPath = item => [item.mountPoint, null]

    const getItems = async (id, requestId) => {
        const response = await request("getroot", { id, requestId });
        const mounted = response.filter(n => n.mountPoint)
        const unmounted = response.filter(n => !n.mountPoint)
        return mounted
            .concat(unmounted)
            .map(n => { 
                n.isNotSelectable = true
                return n
            })
    }

    const getExtendedInfos = () => false

    const saveWidths = widths => localStorage.setItem(`${folderId}-root-widths`, JSON.stringify(widths))

    const getItem = item => item.mountPoint

    return {
        getType,
        getColumns,
        getItems,
        renderRow,
        saveWidths, 
        getCurrentPath,
        getPath,
        getItem,
        getExtendedInfos
    }
}