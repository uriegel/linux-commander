import type { TableColumns } from "virtual-table-react"
import { Root } from "./root"
import type { FolderViewItem } from "../components/FolderView"

export enum IconNameType {
    Parent,
    Root,
    Home,
    Folder,
    File,
    Remote,
    Android,
    New,
    Service,
    Favorite
}

export interface IController {
    id: string 
    getColumns(): TableColumns<FolderViewItem>
}

export function getController(id: string): IController {
    return new Root()
}