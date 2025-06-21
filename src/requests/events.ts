import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"

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
    method: "cmd"|"cmdtoggle"|"status"
    cmd?: string
    checked?: boolean,
    folderId: string,
    requestId?: number,
    text?: string
}

const socket = webSocket<WebSocketMsg>('ws://localhost:20000/events').pipe(share())

export const cmdEvents = socket
                    .pipe(filter(n => n.method == "cmd"))
                    .pipe(map(n => n.cmd || ""))

export const cmdToggleEvents = socket
                    .pipe(filter(n => n.method == "cmdtoggle"))
                    .pipe(map(n => ({
                        cmd: n.cmd || "",
                        checked: n.checked === true
                    })))

export const statusEvents = socket
                    .pipe(filter(n => n.method == "status"))
                    .pipe(map(n => ({
                        folderId: n.folderId || "",
                        requestId: n.requestId || -1,
                        text: n.text
                    })))


statusEvents.subscribe((n: StatusMsg) => console.log("Status", n))                    

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})

