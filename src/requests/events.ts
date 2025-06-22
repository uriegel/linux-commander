import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"
import type { FolderViewItem } from '../components/FolderView'

export type CmdMsg = {
    cmd: string
}
export type CmdToggleMsg = {
    cmd: string,
    checked: boolean
}
export type StatusMsg = {
    folderId: string,
    requestId: number,
    text?: string
}
export type ExifMsg = {
    folderId: string,
    requestId: number,
    items: FolderViewItem[]
}

type WebSocketMsg = {
    method: "cmd" | "cmdtoggle" | "status" | "exifinfo",
    cmdMsg?: CmdMsg,
    cmdToggleMsg?: CmdToggleMsg,
    statusMsg?: StatusMsg
    exifMsg?: ExifMsg
}

const socket = webSocket<WebSocketMsg>('ws://localhost:20000/events').pipe(share())

export const cmdEvents = socket
                    .pipe(filter(n => n.method == "cmd"))
                    .pipe(map(n => n.cmdMsg)!)
export const cmdToggleEvents = socket
                    .pipe(filter(n => n.method == "cmdtoggle"))
                    .pipe(map(n => n.cmdToggleMsg!))
export const statusEvents = socket
                    .pipe(filter(n => n.method == "status"))
                    .pipe(map(n => n.statusMsg!))
export const exifDataEvents = socket
                    .pipe(filter(n => n.method == "exifinfo"))
                    .pipe(map(n => n.exifMsg!))

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})

