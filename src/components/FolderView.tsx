import { forwardRef, useCallback, useEffect, useImperativeHandle, useRef, useState } from "react"
import './FolderView.css'
import VirtualTable, { type SelectableItem, type TableColumns, type VirtualTableHandle } from "virtual-table-react"
import { changePath as changePathRequest } from "../requests/requests"
import { getController, type IController } from "../controllers/controller"
import { Root } from "../controllers/root"

export type FolderViewHandle = {
    id: string
    setFocus: () => void
}

interface ItemCount {
    fileCount: number
    dirCount: number
}

interface FolderViewProp {
    id: string,
    onFocus: () => void
    onItemsChanged: (count: ItemCount) => void
}

enum DriveKind {
    Unknown,
    Ext4,
    Ntfs,
    Vfat,
    Home,
}

export interface FolderViewItem extends SelectableItem {
    name:         string
    size?:        number
    isParent?:    boolean
    isDirectory?: boolean
    // Root item
    description?: string
    mountPoint?:  string
    isMounted?: boolean
    isEjectable?: boolean
    driveKind?: DriveKind
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
    { id, onFocus, onItemsChanged },
    ref) => {
    
    useEffect(() => {
        changePath("root")
    }, []) 

    useImperativeHandle(ref, () => ({
        id,
        setFocus() { virtualTable.current?.setFocus() },
    }))

    const virtualTable = useRef<VirtualTableHandle<FolderViewItem>>(null)
    const itemCount = useRef({ fileCount: 0, dirCount: 0 })

    const [items, setStateItems] = useState([] as FolderViewItem[])

    const controller = useRef<IController>(new Root())
    const refItems = useRef(items) 

    const onPositionChanged = useCallback((item: FolderViewItem) => {}
    , [])         
    //     (item: FolderViewItem) => onPathChanged(controller.current.appendPath(path, item.name),
    //         item.isDirectory == true, item.exifData?.latitude, item.exifData?.longitude
    // ), [path, onPathChanged])         


    // TODO actual item in statuc bar
    // TODO enter => directoryController
    // TODO getItems in DirectoryController

    const setItems = useCallback((items: FolderViewItem[], dirCount?: number, fileCount?: number) => {
        setStateItems(items)
        refItems.current = items
        if (dirCount != undefined || fileCount != undefined) {
            itemCount.current = { dirCount: dirCount || 0, fileCount: fileCount || 0 }
            onItemsChanged(itemCount.current)
        }
    }, [onItemsChanged])


    async function changePath(path: string) {
        const result = await changePathRequest({ id, path })
        if (result.controller) {
            controller.current = getController(result.controller)
            virtualTable.current?.setColumns(setWidths(controller.current.getColumns()))
        }
        setItems(result.items, result.dirCount, result.fileCount)
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

    const onFocusChanged = useCallback(() => {
        onFocus()
        const pos = virtualTable.current?.getPosition() ?? 0
        const item = pos < items.length ? items[pos] : null 
        if (item)
            onPositionChanged(item)
        onItemsChanged(itemCount.current)
    }, [items, onFocus, onPositionChanged, onItemsChanged]) 

    return (
        <div className="folder" onFocus={onFocusChanged}>
            {/* <input ref={input} className="pathInput" spellCheck={false} value={path} onChange={onInputChange} onKeyDown={onInputKeyDown} onFocus={onInputFocus} /> */}
            <div className="tableContainer" >
                <VirtualTable ref={virtualTable} items={items} onColumnWidths={onColumnWidths} />
            </div>
            {/* <RestrictionView items={items} ref={restrictionView} /> */}
        </div>
    )
})

export default FolderView
