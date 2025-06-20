import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"

type WebSocketMsg = {
    method: "cmd"
    cmd?: string
}

const socket = webSocket<WebSocketMsg>('ws://localhost:20000/events')

export const cmdEvents = socket
                    .pipe(share())
                    .pipe(filter(n => n.method == "cmd"))
                    .pipe(map(n => n.cmd || ""))

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})

