
//import { Map, Marker } from "./leaflet"
import "./imageviewer.js"
import { ImageViewer } from "./imageviewer.js"
declare const L: typeof import("./leaflet")

const viewer = document.getElementById('viewer') as ImageViewer
// const viewerContainer = document.getElementById('viewerContainer')
// const viewerDiv = document.getElementById('viewerDiv')

//let previewMode = 0
// let locationLatitude: number|undefined
// let locationLongitude: number|undefined
// let mapdiv: HTMLElement|null = null
// let map: Map|null = null
//let marker: Marker<any>|null = null

registerSetPath(setPath)
registerToggleViewFunc(toggleView)

//function setPath(path: string, lat?: number, long?: number) {
function setPath(path: string) {
    // locationLatitude = lat
    // locationLongitude = long
    
    const smallPath = path.toLowerCase();
    if (smallPath.endsWith(".jpg") || smallPath.endsWith(".jpeg") || smallPath.endsWith(".png"))
    {
        viewer.classList.remove("hidden")
        viewer.path = `/getfile?path=${path}`     
        // if (mapdiv && map) {
        //     if (locationLatitude && locationLongitude) {
        //         mapdiv.classList.remove('hidden')  
        //         map.setView([locationLatitude, locationLongitude])
        //         if (marker)
        //             marker.remove()
        //         marker = L.marker([locationLatitude, locationLongitude]).addTo(map)
        //     } else {
        //         mapdiv.classList.add('hidden')  
        //     }
        // }
    }
    else
        viewer.classList.add("hidden")
}

function toggleView() {
    // if (locationLatitude && locationLongitude) {
    //     previewMode++
    //     if (previewMode > 2)
    //         previewMode = 0

    //     switch (previewMode) {
    //         case 0:
    //             viewerDiv?.classList.remove("hidden")
    //             viewerContainer?.classList.remove("bothViewer")
    //             if (mapdiv)
    //                 viewerContainer?.removeChild(mapdiv)
    //             mapdiv = null
    //             map = null
    //             break
    //         case 1:
    //             viewerContainer?.classList.add("bothViewer")
    //             mapdiv = document.createElement("div")
    //             viewerContainer?.append(mapdiv)
    //             mapdiv.id = "map"
    //             map = L.map('map').setView([locationLatitude, locationLongitude], 13)
    //             L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    //                 maxZoom: 19,
    //                 attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    //             }).addTo(map);
    //             marker = L.marker([locationLatitude, locationLongitude]).addTo(map)
    //             break
    //         case 2:
    //             viewerDiv?.classList.add("hidden")
    //             if (map)
    //                 map.invalidateSize()
    //             break
    //     }
    // }
}