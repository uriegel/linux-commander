import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"

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
type WebSocketMsg = {
    method: "cmd" | "cmdtoggle" | "status",
    cmdMsg?: CmdMsg,
    cmdToggleMsg?: CmdToggleMsg,
    statusMsg?: StatusMsg
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

statusEvents.subscribe((n: StatusMsg) => console.log("Status", n))                    

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})

