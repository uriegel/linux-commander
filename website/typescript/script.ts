import "./imageviewer.js"
import "./trackviewer.js"
import { ImageViewer } from "./imageviewer.js"
import { TrackViewer } from "./trackviewer.js"

const viewer = document.getElementById('imageViewer') as ImageViewer
const mediaPlayer = document.getElementById('mediaPlayer') as HTMLVideoElement
const trackViewer = document.getElementById('trackViewer') as TrackViewer

registerSetPath(setPath)
registerToggleViewFunc(toggleView)

let currentPath: string|null = null

function setPath(path: string, latitude?: number, longitude?: number) {
    currentPath = path
   
    if (isImage(path)) {
        mediaPlayer.classList.add("hidden")
        trackViewer.classList.add("hidden")
        viewer.classList.remove("hidden")
        
        viewer.path = path.startsWith("http") ? path :  `/getfile?path=${path}`
        viewer.location = latitude && longitude ? { latitude, longitude } : null
    }
    else if (isMedia(path)) {
        viewer.classList.add("hidden")
        trackViewer.classList.add("hidden")
        mediaPlayer.classList.remove("hidden")
        
        mediaPlayer.src = path.startsWith("http") ? path :  `/getfile?path=${path}`
    }
    else if (isTrack(path)) {
        viewer.classList.add("hidden")
        mediaPlayer.classList.add("hidden")
        trackViewer.classList.remove("hidden")
        trackViewer.path = path
    }
    else {
        viewer.classList.add("hidden")
        trackViewer.classList.add("hidden")
        mediaPlayer.classList.add("hidden")
    }
}

function toggleView() {
    if (isImage(currentPath))
        viewer.toggleView()
}

function isImage(path: string | null): boolean {
    const smallPath = path?.toLowerCase()
    return smallPath?.endsWith(".jpg") || smallPath?.endsWith(".jpeg") || smallPath?.endsWith(".png") || false
}

function isMedia(path: string | null): boolean {
    const smallPath = path?.toLowerCase()
    return smallPath?.endsWith(".mp4") || smallPath?.endsWith(".mkv") || smallPath?.endsWith(".mp3") || false
}

function isTrack(path: string | null): boolean {
    const smallPath = path?.toLowerCase()
    return smallPath?.endsWith(".gpx") || false
}
