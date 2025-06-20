import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"

type WebSocketMsg = {
    method: "cmd"
    cmd?: string
}

const socket = webSocket<WebSocketMsg>('ws://localhost:20000/events')

const commanderEvents = socket.pipe(share())
export const cmdEvents = socket
                    .pipe(filter(n => n.method == "cmd"))
                    .pipe(map(n => n.cmd || ""))

commanderEvents.subscribe({
    next: message => console.log('Received:', message),
})

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})

