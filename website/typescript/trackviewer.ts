import { LatLngExpression, Map, Marker, Polyline } from "./leaflet"
declare const L: typeof import("./leaflet")

type TrackInfo = {
    name?: string
    description?: string
    distance: number,
    duration: number,
    averageSpeed: number,
    averageHeartRate: number,
    maxSpeed: number,
    maxHeartRate: number,
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
        this.trackPoints = track?.trackPoints?.map(n => [n.latitude!, n.longitude!])
        if (this.trackPoints) {
            if (!this.map) {
                this.map = L.map('trackContainer').setView([0, 0], 13)
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                }).addTo(this.map)

                const SliderControl = L.Control.extend({
                    onAdd: () => {
                        const container = L.DomUtil.create("div", "leaflet-bar leaflet-control")
                        container.classList.add("trackSlider")
                     
                        this.slider = L.DomUtil.create("input", "", container)
                        this.slider.type = "range"
                        this.slider.min = '0'
                        this.slider.max = `${this.trackPoints?.length}`
                        this.slider.value = '0'
                                              
                        // Prevent map zoom when interacting with slider
                        L.DomEvent.disableClickPropagation(container)
                      
                        this.slider.addEventListener("input", () => {
                            let percent = parseInt(this.slider!.value)
                            this.marker?.setLatLng([this.trackPoints![percent][0], this.trackPoints![percent][1]], )
                        })
                  
                        return container
                    },
                })
                this.map.addControl(new SliderControl({ position: "bottomleft" }))                

                const TableControl = L.Control.extend({
                    onAdd: () => {
                        const container = L.DomUtil.create("div", "leaflet-bar leaflet-control")
                        container.classList.add("trackStatistics")
                        const template = document.getElementById("track-statistics") as HTMLTemplateElement
                        container.appendChild(template.content.cloneNode(true)) 
                        this.statistics = container.querySelector("table") as HTMLTableElement
                        return container
                    },
                });
                this.map.addControl(new TableControl({ position: "topright" }))
            }    

            this.slider!.value = '0'
            this.slider!.max = `${this.trackPoints?.length}`
            
            const maxLat = this.trackPoints.reduce((prev, curr) => Math.max(prev, curr[0]), this.trackPoints[0][0])
            const minLat = this.trackPoints.reduce((prev, curr) => Math.min(prev, curr[0]), this.trackPoints[0][0])
            const maxLng = this.trackPoints.reduce((prev, curr) => Math.max(prev, curr[1]), this.trackPoints[0][1])
            const minLng = this.trackPoints.reduce((prev, curr) => Math.min(prev, curr[1]), this.trackPoints[0][1])
            this.map?.fitBounds([[maxLat, maxLng], [minLat, minLng]])
            if (this.track)
                this.track.remove()
            this.track = L.polyline(this.trackPoints as LatLngExpression[]).addTo(this.map)
            if (this.marker)
                this.marker.remove()
            this.marker = L.marker([this.trackPoints[0][0], this.trackPoints[0][1]], { autoPan: true }).addTo(this.map)

            this.addStatisticValue(".dist", track?.distance)
            this.addStatisticValue(".duration", track?.duration)
            this.addStatisticValue(".averageSpeed", track?.averageSpeed)
            this.addStatisticValue(".averageHeartRate", track?.averageHeartRate)
            this.addStatisticValue(".maxHeartRate", track?.maxHeartRate)
        }
    }

    addStatisticValue(cellClass: string, val?: number) {
        if (val) {
            const span = this.statistics?.querySelector(`${cellClass} span`) as HTMLSpanElement
            span.innerText = `${val}`
            this.statistics?.querySelector(cellClass)?.classList.remove("hidden")
        } else
            this.statistics?.querySelector(cellClass)?.classList.add("hidden")
    }

    map: Map | null = null
    track: Polyline<any> | null = null
    trackPoints: number[][] | undefined
    marker: Marker<any> | null = null    
    slider: HTMLInputElement | undefined
    statistics: HTMLTableElement|undefined
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