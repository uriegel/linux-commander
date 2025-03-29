import { Map } from "./leaflet"
declare const L: typeof import("./leaflet")

type TrackInfo = {
    name?: string
    description?: string
    distance: number,
    duration: number,
    averageSpeed: number,
    averageHeartRate: number,
    trackPoints?: TrackPoint[] 
}

type TrackPoint = {
    latitude?: number
    longitude?: number
    elevation?: number
    time?: Date
    heartrate?: number
    velocity?: number
}


export class TrackViewer extends HTMLElement {
    constructor() {
        super()

        const link = document.createElement("link")
        link.rel = "stylesheet"
        link.href = "../styles/trackviewer.css"
        this.appendChild(link)

        let template = document.getElementById("track-viewer") as HTMLTemplateElement
        let templateContent = template.content
        this.appendChild(templateContent.cloneNode(true))
    }

    set path(val: string) {
        this.setPath(val)
    }

    async setPath(path: string) {
        const track = await getTrack(path)
        const trk = track?.trackPoints?.map(n => [n.latitude!, n.longitude!])
        if (trk) {
            if (!this.map) {
                this.map = L.map('trackContainer').setView([0, 0], 13)
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                }).addTo(this.map)
            }    

            const maxLat = trk.reduce((prev, curr) => Math.max(prev, curr[0]), trk[0][0])
            const minLat = trk.reduce((prev, curr) => Math.min(prev, curr[0]), trk[0][0])
            const maxLng = trk.reduce((prev, curr) => Math.max(prev, curr[1]), trk[0][1])
            const minLng = trk.reduce((prev, curr) => Math.min(prev, curr[1]), trk[0][1])
            this.map?.fitBounds([[maxLat, maxLng], [minLat, minLng]])
        }
    }

    map: Map | null = null
}

async function getTrack(path: string): Promise<TrackInfo|null> {
    try {
        const response = await fetch(`/gettrack?path=${path}`)
        return await response.json() as TrackInfo
    }
    catch (e) {
        return null
    }
}

customElements.define('track-viewer', TrackViewer)