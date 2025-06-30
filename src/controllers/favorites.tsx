import type { TableColumns } from "virtual-table-react"
import { IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"
import { ResultType, type DialogHandle } from "web-dialog-react"

export const FAVORITES = "fav"

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
        const items = this.getFavoriteItems()
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
        if (enterData.item.isParent)
            return {
                processed: false,
                pathToSet: "root"
            }
        else if (enterData.item.isNew && enterData.dialog && enterData.otherPath && await this.createFavorite(enterData.dialog, enterData.otherPath))
            return {
                processed: true,
                refresh: true
            }
        else
            return {
                processed: false,
                pathToSet: enterData.item.name
            }
    }

    sort(items: FolderViewItem[]) { return items }

    itemsSelectable: boolean

    onSelectionChanged() { }
    
    getCopyText() { return "" }
    
    async deleteItems(items: FolderViewItem[], dialog: DialogHandle) {
        const itemsToDelete = items.filter(n => !n.isNew && !n.isParent)
        if (itemsToDelete.length == 0)
            return { success: true }
        const res = await dialog.show({
		    text: `Möchtest Du ${itemsToDelete.length > 1 ? "die Favoriten" : "den Favoriten"} löschen?`,
    		btnOk: true,
	    	btnCancel: true,
		    defBtnOk: true
        })
        if (res.result != ResultType.Ok)
            return { success: true }
        
        const favs = this.getFavoriteItems().filter(x => !items.find(n => n.name == x.name))
        localStorage.setItem("fav", JSON.stringify(favs))
        return { success: true, refresh: true }
    }

    async createFavorite(dialog: DialogHandle, otherPath: string) {
        const items = this.getFavoriteItems()
        const result =
            !items.find(n => n.name == otherPath) && (await dialog.show({
                text: `'${otherPath}' als Favoriten hinzufügen?`,
                btnOk: true,
                btnCancel: true,
                defBtnOk: true
            }))?.result == ResultType.Ok
        if (result && otherPath) {
            const newItems = items.concat([{ name: otherPath }])
            localStorage.setItem(FAVORITES, JSON.stringify(newItems))
            return true
        }
        else
            return false
    }

    getFavoriteItems() {
        const itemsStr = localStorage.getItem(FAVORITES)
        return itemsStr ? JSON.parse(itemsStr) as FolderViewItem[] : []
    }

    async rename(_: DialogHandle, __: FolderViewItem) { return false }
    
    constructor() {
        this.id = "FAV"
        this.itemsSelectable = true
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
