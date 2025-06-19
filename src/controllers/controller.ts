import type { TableColumns } from "virtual-table-react"
import { Root } from "./root"
import type { FolderViewItem } from "../components/FolderView"

export enum IconNameType {
    Parent,
    Root,
    RootEjectable,
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
    appendPath(path: string, subPath: string): string
}

export function getController(id: string): IController {
    return new Root()
}

export const formatSize = (num?: number) => {
    if (!num)
        return ""
    let sizeStr = num.toString()
    const sep = '.'
    if (sizeStr.length > 3) {
        const sizePart = sizeStr
        sizeStr = ""
        for (let j = 3; j < sizePart.length; j += 3) {
            const extract = sizePart.slice(sizePart.length - j, sizePart.length - j + 3)
            sizeStr = sep + extract + sizeStr
        }
        const strfirst = sizePart.substring(0, (sizePart.length % 3 == 0) ? 3 : (sizePart.length % 3))
        sizeStr = strfirst + sizeStr
    }
    return sizeStr    
}
