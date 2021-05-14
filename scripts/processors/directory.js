export const DIRECTORY = "directory"

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

    const getItems = async path => {
        const responseStr = await fetch(`/commander/getitems?path=${path}`)
        const response = await responseStr.json()
        const mounted = response.filter(n => n.mountPoint)
        const unmounted = response.filter(n => !n.mountPoint)
        return mounted
            .concat(unmounted)
            .map(n => { 
                n.isNotSelectable = true
                return n
            })
    }    

    const saveWidths = widths => localStorage.setItem(`${folderId}-directory-widths`, JSON.stringify(widths))

    return {
        getType,
        getColumns,
        getItems,
        saveWidths
    }
}