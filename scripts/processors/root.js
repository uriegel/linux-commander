export const getRoot = folderId => {
    const getColumns = () => {
        const widthstr = localStorage.getItem(`${folderId}-widths`)
        const widths = widthstr ? JSON.parse(widthstr) : []
        let columns = [{
            name: "Name",
            render: (td, item) => td.innerHTML = item.ext
        }, {
            name: "Dateisystem",
            render: (td, item) => td.innerHTML = item.ext
        }, {
            name: "Mountpoint",
            render: (td, item) => td.innerHTML = item.ext
        }, {
            name: "Größe",
            isRightAligned: true,
            render: (td, item) => {
                td.innerHTML = item.size
                td.classList.add("rightAligned")
            }
        }]
        if (widths)
            columns = columns.map((n, i)=> ({ ...n, width: widths[i]}))
        return columns
    }

    const saveWidths = widths => localStorage.setItem(`${folderId}-widths`, JSON.stringify(widths))

    return {
        getColumns,
        saveWidths
    }
}