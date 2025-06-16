import { forwardRef, useImperativeHandle } from "react"
import './FolderView.css'

export type FolderViewHandle = {
    id: string
}

interface FolderViewProp {
    id: string
}

const FolderView = forwardRef<FolderViewHandle, FolderViewProp>((
    { id },
    ref) => {

    useImperativeHandle(ref, () => ({
        id
    }))

    return (
        <div>EinFolderView</div>
        // <div className={`folder${dragging ? " dragging" : ""}`} onFocus={onFocusChanged}
        //     onDragEnter={isWindows() ? onDragEnter : undefined} onDragOver={isWindows() ? onDragOver : undefined}
        //     onDragLeave={isWindows() ? onDragLeave : undefined} onDrop={isWindows() ? onDrop : undefined}>
        //     <input ref={input} className="pathInput" spellCheck={false} value={path} onChange={onInputChange} onKeyDown={onInputKeyDown} onFocus={onInputFocus} />
        //     <div className={`tableContainer${dragStarted ? " dragStarted" : ""}`} onKeyDown={onKeyDown} >
        //         <VirtualTable ref={virtualTable} items={items} onSort={onSort} onColumnWidths={onColumnWidths} onItemClick={onItemClick}
        //             onDragStart={isWindows() ? onDragStart : undefined} onEnter={onEnter} onPosition={onPositionChanged} />
        //     </div>
        //     <RestrictionView items={items} ref={restrictionView} />
        // </div>
    )
})

export default FolderView
