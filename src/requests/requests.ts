import type { FolderViewItem } from "../components/FolderView"

interface ChangePath {
    id: string,
    path?: string
    mount?: boolean
    showHidden?: boolean
}

interface ChangePathResponse {
    cancelled?: boolean
    id: number
    controller?: string,
    dirCount: number,
    fileCount: number,
    items: FolderViewItem[],
    path?: string
}

interface PrepareCopy {
    id: string,
    path: string,
    targetPath: string,
    move: boolean,
    items: FolderViewItem[] 
}

export interface PrepareCopyResponse {
    selectedItemsType: SelectedItemsType,
    totalSize: number,
    conflicts: CopyItem[]
}

export interface CopyItem {
    source: FolderViewItem,
    target: FolderViewItem
}

interface Copy {
    id: string
    cancelled?: boolean
    notOverwrite?: boolean
}

interface CopyResponse {
    cancelled: boolean
}

interface Delete {
    id: string
    path: string,
    items: FolderViewItem[]
}

interface DeleteResponse {
    success: boolean
}

export const SelectedItemsType = {
    None: 0,
    Folder: 1,
    Folders: 2,
    File: 3,
    Files: 4,
    Both: 5
}
export type SelectedItemsType = (typeof SelectedItemsType)[keyof typeof SelectedItemsType]

export const changePath = getJsonPost<ChangePath, ChangePathResponse>("changepath")
export const prepareCopy = getJsonPost<PrepareCopy, PrepareCopyResponse>("preparecopy")
export const copy = getJsonPost<Copy, CopyResponse>("copy")
export const deleteRequest = getJsonPost<Delete, DeleteResponse>("delete")

function getJsonPost<RequestType, ResponseType>(method: string): (request: RequestType) => Promise<ResponseType> {
 
    async function jsonPost<RequestType, ResponseType>(method: string, request: RequestType): Promise<ResponseType> {
        const msg = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        }

        const response = await fetch(`http://localhost:20000/request/${method}`, msg)
        const json = await response.text()
        return JSON.parse(json) as ResponseType
    }

    return (request: RequestType) => jsonPost(method, request)
}

