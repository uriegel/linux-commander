import type { TableColumns } from "virtual-table-react"
import { formatSize, IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"

export class Root implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
                { name: "Name" },
                { name: "Bezeichnung" },
                { name: "Mountpoint" },
                { name: "Größe", isRightAligned: true }
            ],
            getRowClasses,
            renderRow
        }
    }

    getItems(): FolderViewItem[] { throw "not implemented" }

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

    sort(items: FolderViewItem[]) { return items }

    itemsSelectable: boolean

    onSelectionChanged() { }
    
    getCopyText() { return "" }
    async deleteItems() { return { success: true }}
        
    constructor() {
        this.id = "ROOT"
        this.itemsSelectable = false
    }
}

const REMOTES = "remotes"
const FAVORITES = "fav"

const getRowClasses = (item: FolderViewItem) => 
    item.isMounted == false
        ? ["notMounted"]
        : []

const renderRow = (item: FolderViewItem) => [
    (<IconName namePart={item.name} type={
        item.name == '~'
        ? IconNameType.Home
        : item.name == REMOTES
        ? IconNameType.Remote
        : item.name == FAVORITES
        ? IconNameType.Favorite
        : item.isEjectable ? IconNameType.RootEjectable : IconNameType.Root
    } />),
    item.description ?? "",
    item.mountPoint ?? "",
    formatSize(item.size)
]
