import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react"
import './FolderView.css'
import VirtualTable, { type SelectableItem, type TableColumns, type VirtualTableHandle } from "virtual-table-react"
import { changePath as changePathRequest } from "../requests/requests"
import { getController, type IController } from "../controllers/controller"
import { Root } from "../controllers/root"

export type FolderViewHandle = {
    id: string
}

interface FolderViewProp {
    id: string
}

export interface FolderViewItem extends SelectableItem {
    name:         string
    size?:        number
    isParent?:    boolean
    isDirectory?: boolean
    // Root item
    description?: string
    mountPoint?:  string
    isMounted?:   boolean
    // FileSystem item
    iconPath?:    string
    time?:        string
    // exifData?:    ExifData
    isHidden?:    boolean
    // Remotes item
    ipAddress?:   string
    isAndroid?:   boolean
    isNew?: boolean
    // ExtendedRename
    newName?:     string|null
    // Favorites
    path?: string | null
}

const FolderView = forwardRef<FolderViewHandle, FolderViewProp>((
    { id },
    ref) => {
    
    useEffect(() => {
        changePath("root")
    }, []) 

    useImperativeHandle(ref, () => ({
        id
    }))

    const virtualTable = useRef<VirtualTableHandle<FolderViewItem>>(null)
    const [items, setStateItems] = useState([] as FolderViewItem[])

    const controller = useRef<IController>(new Root())

    async function changePath(path: string) {
        const result = await changePathRequest({ id, path })
        if (result.controller) {
            controller.current = getController(result.controller)
            virtualTable.current?.setColumns(setWidths(controller.current.getColumns()))
        }
    }

    const getWidthsId = () => `${id}-${controller.current.id}-widths`

    const onColumnWidths = (widths: number[]) => {
        if (widths.length)
            localStorage.setItem(getWidthsId(), JSON.stringify(widths))
    }

    const setWidths = (columns: TableColumns<FolderViewItem>) => {
        const widthstr = localStorage.getItem(getWidthsId())
        const widths = widthstr ? JSON.parse(widthstr) as number[] : null
        return widths
            ? {
                ...columns, columns: columns.columns.map((n, i) => ({...n, width: widths![i]}))
            }
            : columns
    }

    return (
        <div className="folder">
            {/* <input ref={input} className="pathInput" spellCheck={false} value={path} onChange={onInputChange} onKeyDown={onInputKeyDown} onFocus={onInputFocus} /> */}
            <div className="tableContainer" >
                <VirtualTable ref={virtualTable} items={items} onColumnWidths={onColumnWidths} />
            </div>
            {/* <RestrictionView items={items} ref={restrictionView} /> */}
        </div>
    )
})

export default FolderView
