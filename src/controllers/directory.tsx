import type { TableColumns } from "virtual-table-react"
import { formatDateTime, formatSize, IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
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

    constructor() {
        this.id = "ROOT"
    }
}

const REMOTES = "remotes"
const FAVORITES = "fav"

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

