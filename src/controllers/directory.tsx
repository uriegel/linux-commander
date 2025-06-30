import type { TableColumns } from "virtual-table-react"
import { formatDateTime, formatSize, IconNameType, sortItems, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"
import "../extensions/extensions"
import { onEnter, SelectedItemsType, type PrepareCopyResponse } from "../requests/requests"
import type { DialogHandle } from "web-dialog-react"

export interface ExtendedRenameProps {
    prefix: string
    digits: number
    startNumber: number
}

export class Directory implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
    	        { name: "Name", isSortable: true, subColumn: "Erw." },
		        { name: "Datum", isSortable: true },
                { name: "Größe", isSortable: true, isRightAligned: true }
            ],
            getRowClasses: this.getRowClasses,
            renderRow
        }
    }

    getItems(): FolderViewItem[] { throw "not implemented" }

    appendPath(path: string, subPath: string) {
        return path.appendPath(subPath)
    } 

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        
        if (!enterData.item.isDirectory) {
            await onEnter({ id: enterData.id ?? "", name: enterData.item.name, path: enterData.path })
            return {
                processed: true
            }
        }
        else
            return {
                processed: false,
                pathToSet: this.appendPath(enterData.path, enterData.item.name),
                latestPath: enterData.item.isParent ? enterData.path.extractSubPath() : undefined 
            }
    }

    sort(items: FolderViewItem[], sortIndex: number, sortDescending: boolean) {
        return sortItems(items, this.getSortFunction(sortIndex, sortDescending))     
    }

    getSortFunction = (index: number, descending: boolean) => {
        const ascDesc = (sortResult: number) => descending ? -sortResult : sortResult
        const sf = index == 0
            ? (a: FolderViewItem, b: FolderViewItem) => a.name.localeCompare(b.name) 
            : index == 1
                ? (a: FolderViewItem, b: FolderViewItem) => {	
                    const aa = a.exifData?.dateTime ? a.exifData?.dateTime : a.time || ""
                    const bb = b.exifData?.dateTime ? b.exifData?.dateTime : b.time || ""
                    return aa.localeCompare(bb) 
                } 
            : index == 2
            ? (a: FolderViewItem, b: FolderViewItem) => (a.size || 0) - (b.size || 0)
            : index == 10
            ? (a: FolderViewItem, b: FolderViewItem) => a.name.getExtension().localeCompare(b.name.getExtension()) 
            : undefined
        
        return sf
            ? (a: FolderViewItem, b: FolderViewItem) => ascDesc(sf(a, b))
            : undefined
    }

    itemsSelectable: boolean

    onSelectionChanged(_: FolderViewItem[]) { }
    
    getCopyText(prepareCopy: PrepareCopyResponse, move: boolean) {
        const copyAction = `${move ? "verschieben" : " kopieren"} (${prepareCopy.totalSize.byteCountToString()})`
        return prepareCopy.conflicts.length > 0
            ? `Einträge überschreiben beim ${move ? "Verschieben" : "Kopieren"} (${prepareCopy.totalSize.byteCountToString()}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.File
            ? `Möchtest Du die Datei ${copyAction}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.Folder
            ? `Möchtest Du das Verzeichnis ${copyAction}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.Files
            ? `Möchtest Du die Dateien ${copyAction}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.Folders
            ? `Möchtest Du die Verzeichnisse ${copyAction}?`
            : `Möchtest Du die Einträge ${copyAction}?`
    }

    async deleteItems(items: FolderViewItem[]) {
        const deleteText = this.getDeleteText(items)
        return deleteText
            ? ({
                deleteText
            })
            : ({
                success: true
            })
    }

    getDeleteText(items: FolderViewItem[]) { 
        const dirs = items.filter(n => n.isDirectory).length
        const files = items.filter(n => !n.isDirectory).length
        return dirs > 0 && files > 0
            ? "Möchtest Du die Dateien und Verzeichnisse löschen?"
            : dirs == 1 && files == 0
            ? "Möchtest Du das Verzeichnis löschen?"
            : dirs == 0 && files == 1
            ? "Möchtest Du die Datei löschen?"
            : dirs > 1 && files == 0
            ? "Möchtest Du die Verzeichnisse löschen?"
            : dirs == 0 && files > 1
            ? "Möchtest Du die Dateien löschen?"
            : ""                
    }   

    async rename(_: DialogHandle, __: FolderViewItem) { return false }

    getRowClasses(item: FolderViewItem) {
        return item.isHidden
            ? ["hidden"]
            : []
    }

    constructor() {
        this.id = "FILE"
        this.itemsSelectable = true
    }
}

// TODO const REMOTES = "remotes"
// TODO const FAVORITES = "fav"

const renderRow = (item: FolderViewItem) => [
	(<IconName namePart={item.name} type={
			item.isParent
			? IconNameType.Parent
			: item.isDirectory
			? IconNameType.Folder
			: IconNameType.File}
		iconPath={item.name.getExtension()} />),
	(<span className={item.exifData?.dateTime ? "exif" : "" } >{formatDateTime(item?.exifData?.dateTime ?? item?.time)}</span>),
	formatSize(item.size)
]


