import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react"
import './FolderView.css'
import VirtualTable, { type SelectableItem, type VirtualTableHandle } from "virtual-table-react"
import { jsonPost } from "../requests/requests"

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
        const init = async () => {
            const result = await jsonPost("init", {})
            console.log("init", result)
        }
        init()
    }, []) 

    useImperativeHandle(ref, () => ({
        id
    }))

    const virtualTable = useRef<VirtualTableHandle<FolderViewItem>>(null)
    const [items, setStateItems] = useState([] as FolderViewItem[])

    return (
        <div className="folder">
            {/* <input ref={input} className="pathInput" spellCheck={false} value={path} onChange={onInputChange} onKeyDown={onInputKeyDown} onFocus={onInputFocus} /> */}
            <div className="tableContainer" >
                <VirtualTable ref={virtualTable} items={items} />
            </div>
            {/* <RestrictionView items={items} ref={restrictionView} /> */}
        </div>
    )
})

export default FolderView
