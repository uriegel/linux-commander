import type { TableColumns } from "virtual-table-react"
import { IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"

export class Favorites implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
                { name: "Name" },
                { name: "Pfad" },
            ],
            getRowClasses: () => [],
            renderRow
        }
    }

    getItems(): FolderViewItem[] { 
        const itemsStr = localStorage.getItem("fav")
        const items = itemsStr ? JSON.parse(itemsStr) as FolderViewItem[] : []
        return addParent(items)
            .concat({
                name: "Favoriten hinzufügen...",
                isNew: true
            })
    }

    appendPath(_: string, subPath: string) {
        return subPath
    } 

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        console.log("ernter", enterData)
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
    getDeleteText() { return "" }
    
    constructor() {
        this.id = "FAV"
        this.itemsSelectable = false
    }
}

const renderRow = (item: FolderViewItem) => [
    (<IconName namePart={item.name} type={
        item.isParent
        ? IconNameType.Parent
        : item.isNew
        ? IconNameType.New
        : IconNameType.Favorite}
        iconPath={item.name.getExtension()}
    />),
    item.path ?? ""
]

export const addParent = (items: FolderViewItem[]) => 
    [{ name: "..", index: 0, isParent: true, isDirectory: true } as FolderViewItem]
        .concat(items)
