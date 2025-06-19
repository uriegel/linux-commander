import type { TableColumns } from "virtual-table-react"
import { formatDateTime, formatSize, IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"

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

    appendPath(_: string, subPath: string) {
        return subPath
    } 

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        return {
            processed: false,
            pathToSet: enterData.item.mountPoint || enterData.item.mountPoint!.length > 0 ? enterData.item.mountPoint : enterData.item.name,
            mount: !enterData.item.mountPoint            
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
		iconPath={item.iconPath} />),
	(<span className={item.exifData?.dateTime ? "exif" : "" } >{formatDateTime(item?.exifData?.dateTime ?? item?.time)}</span>),
	formatSize(item.size)
]

