import type { TableColumns } from "virtual-table-react"
import { IconNameType, type IController } from "./controller"
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

    constructor() {
        this.id = "ROOT"
    }
}

const getRowClasses = (item: FolderViewItem) => 
    item.isMounted == false
        ? ["notMounted"]
        : []

const renderRow = (item: FolderViewItem) => [
    (<IconName namePart={item.name} type={
        item.name == '~'
        ? IconNameType.Home
        // : item.name == REMOTES
        // ? IconNameType.Remote
        // : item.name == FAVORITES
        // ? IconNameType.Favorite
        : IconNameType.Root
    } />),
    item.description ?? "",
    item.mountPoint ?? "",
    formatSize(item.size)
]
