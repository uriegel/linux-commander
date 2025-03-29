import "./imageviewer.js"
import { ImageViewer } from "./imageviewer.js"

const viewer = document.getElementById('imageViewer') as ImageViewer

registerSetPath(setPath)
registerToggleViewFunc(toggleView)

let currentPath: string|null = null

function setPath(path: string, latitude?: number, longitude?: number) {
    currentPath = path
   
    if (isImage(path)) {
        viewer.classList.remove("hidden")
        viewer.path = `/getfile?path=${path}`     
        viewer.location = latitude && longitude ? { latitude, longitude} : null
    }
    else
        viewer.classList.add("hidden")
}

function toggleView() {
    if (isImage(currentPath))
        viewer.toggleView()
}

function isImage(path: string | null): boolean {
    const smallPath = path?.toLowerCase()
    return smallPath?.endsWith(".jpg") || smallPath?.endsWith(".jpeg") || smallPath?.endsWith(".png") || false
}