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

export const changePath = getJsonPost<ChangePath, ChangePathResponse>("changepath")

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

