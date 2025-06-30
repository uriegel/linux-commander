import type { TableColumns } from "virtual-table-react"
import { IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"
import { ResultType, type DialogHandle } from "web-dialog-react"
import RemoteDialog from "../components/dialogparts/RemoteDialog"

export const REMOTES = "remotes"

export class Remotes implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
                { name: "Name" },
                { name: "IP-Adresse" }
            ],
            getRowClasses: () => [],
            renderRow
        }
    }

    getItems(): FolderViewItem[] { 
        const items = this.getRemoteItems()
        return addParent(items)
            .concat({
                name: "Entferntes Gerät hinzufügen...",
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
        else if (enterData.item.isNew && enterData.dialog && await this.createRemote(enterData.dialog))
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
		    text: `Möchtest Du ${itemsToDelete.length > 1 ? "die Geräte" : "das Gerät"} löschen?`,
    		btnOk: true,
	    	btnCancel: true,
		    defBtnOk: true
        })
        if (res.result != ResultType.Ok)
            return { success: true }
        
        const favs = this.getRemoteItems().filter(x => !items.find(n => n.name == x.name))
        localStorage.setItem(REMOTES, JSON.stringify(favs))
        return { success: true, refresh: true }
    }

    async createRemote(dialog: DialogHandle, item?: FolderViewItem) {
        let name = item?.name
        let ipAddress = item?.ipAddress
        let isAndroid = item?.isAndroid ?? true
        const items = this.getRemoteItems().filter(n => n.name != item?.name)
        const result = await dialog.show({
            text: "Entferntes Gerät hinzufügen",
            extension: RemoteDialog,
            extensionProps: { name, ipAddress, isAndroid },
            onExtensionChanged: (e: FolderViewItem) => {
                name = e.name
                ipAddress = e.ipAddress
                isAndroid = e.isAndroid ?? false
            },
            btnOk: true,
            btnCancel: true,
            defBtnOk: true
        })
        if (name && result.result == ResultType.Ok ) {
            const newItems = items.concat([{ name, ipAddress, isAndroid }])
            localStorage.setItem(REMOTES, JSON.stringify(newItems))
            return true
        }
        else
            return false
    }

    getRemoteItems() {
        const itemsStr = localStorage.getItem(REMOTES)
        return itemsStr ? JSON.parse(itemsStr) as FolderViewItem[] : []
    }
    
    constructor() {
        this.id = "REMOTES"
        this.itemsSelectable = true
    }
}

const renderRow = (item: FolderViewItem) => [
    (<IconName namePart={item.name} type={
        item.isParent
        ? IconNameType.Parent
        : item.isNew
        ? IconNameType.New
        : item.isAndroid
        ? IconNameType.Android
        : IconNameType.Remote}
        iconPath={item.name.getExtension()}
    />),
    item.ipAddress ?? ""
]

export const addParent = (items: FolderViewItem[]) => 
    [{ name: "..", index: 0, isParent: true, isDirectory: true } as FolderViewItem]
        .concat(items)
