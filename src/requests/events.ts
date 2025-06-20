import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"

export type CmdToggleMsg = {
    cmd: string,
    checked: boolean
}

type WebSocketMsg = {
    method: "cmd"|"cmdtoggle"
    cmd?: string
    checked?: boolean
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

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})

