import { Map, Marker } from "./leaflet"
declare const L: typeof import("./leaflet")

export type Location = {
    latitude: number,
    longitude: number
}

export class ImageViewer extends HTMLElement {
    constructor() {
        super()

        const link = document.createElement("link")
        link.rel = "stylesheet"
        link.href = "../styles/imageviewer.css"
        this.appendChild(link)

        let template = document.getElementById("image-viewer") as HTMLTemplateElement
        let templateContent = template.content
        this.appendChild(templateContent.cloneNode(true))
        this.image = document.getElementById('viewer') as HTMLImageElement
        this.viewerContainer = document.getElementById('viewerContainer')
        this.viewerDiv = document.getElementById('viewerDiv')
    }

    set path(val: string) {
        this.image.src = val
    }

    set location(val: Location|null) {
        this.locationVal = val
        if (this.mapdiv && this.map) {
            if (this.locationVal?.latitude && this.locationVal.longitude) {
                this.mapdiv.classList.remove('hidden')  
                this.map.setView([this.locationVal.latitude, this.locationVal.longitude])
                if (this.marker)
                    this.marker.remove()
                this.marker = L.marker([this.locationVal.latitude, this.locationVal.longitude]).addTo(this.map)
            } else {
                this.mapdiv.classList.add('hidden')  
            }
        }
    }

    toggleView() {
        if (this.locationVal) {
            this.viewMode++
            if (this.viewMode > 2)
                this.viewMode = 0
    
            switch (this.viewMode) {
                case 0:
                    this.viewerDiv?.classList.remove("hidden")
                    this.viewerContainer?.classList.remove("bothViewer")
                    if (this.mapdiv)
                        this.viewerContainer?.removeChild(this.mapdiv)
                    this.mapdiv = null
                    this.map = null
                    break
                case 1:
                    this.viewerContainer?.classList.add("bothViewer")
                    this.mapdiv = document.createElement("div")
                    this.viewerContainer?.append(this.mapdiv)
                    this.mapdiv.id = "map"
                    this.map = L.map('map').setView([this.locationVal.latitude, this.locationVal.longitude], 13)
                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        maxZoom: 19,
                        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                    }).addTo(this.map)
                    this.marker = L.marker([this.locationVal.latitude, this.locationVal.longitude]).addTo(this.map)
                    break
                case 2:
                    this.viewerDiv?.classList.add("hidden")
                    if (this.map)
                        this.map.invalidateSize()
                    break
            }
        }
    }        

    image: HTMLImageElement
    locationVal: Location|null = null
    viewerContainer: HTMLElement|null
    viewerDiv: HTMLElement|null
    mapdiv: HTMLElement | null = null
    map: Map | null = null
    marker: Marker<any>|null = null
    viewMode = 0
}

customElements.define('image-viewer', ImageViewer)