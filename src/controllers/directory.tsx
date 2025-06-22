import type { TableColumns } from "virtual-table-react"
import { formatDateTime, formatSize, IconNameType, sortItems, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"
import "../extensions/extensions"

export class Directory implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
    	        { name: "Name", isSortable: true, subColumn: "Erw." },
		        { name: "Datum", isSortable: true },
                { name: "Größe", isSortable: true, isRightAligned: true }
            ],
            getRowClasses,
            renderRow
        }
    }

    appendPath(path: string, subPath: string)  {
        return path.appendPath(subPath)
    } 

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        
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

    onSelectionChanged() {}

    constructor() {
        this.id = "ROOT"
        this.itemsSelectable = true
    }
}

// TODO const REMOTES = "remotes"
// TODO const FAVORITES = "fav"

const getRowClasses = (item: FolderViewItem) => 
	item.isHidden
		? ["hidden"]
		: []

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

