export class ImageViewer extends HTMLElement {
    constructor() {
        super();
        const link = document.createElement("link");
        link.rel = "stylesheet";
        link.href = "../styles/imageviewer.css";
        this.appendChild(link);
        let template = document.getElementById("image-viewer");
        let templateContent = template.content;
        this.appendChild(templateContent.cloneNode(true));
        this.image = document.getElementById('viewer');
        this.viewerContainer = document.getElementById('viewerContainer');
        this.viewerDiv = document.getElementById('viewerDiv');
    }
    set path(val) {
        this.image.src = val;
    }
    set location(val) {
        this.locationVal = val;
        if (this.mapdiv && this.map) {
            if (this.locationVal?.latitude && this.locationVal.longitude) {
                this.mapdiv.classList.remove('hidden');
                this.map.setView([this.locationVal.latitude, this.locationVal.longitude]);
                if (this.marker)
                    this.marker.remove();
                this.marker = L.marker([this.locationVal.latitude, this.locationVal.longitude]).addTo(this.map);
            }
            else {
                this.mapdiv.classList.add('hidden');
            }
        }
    }
    toggleView() {
        if (this.locationVal) {
            this.viewMode++;
            if (this.viewMode > 2)
                this.viewMode = 0;
            switch (this.viewMode) {
                case 0:
                    this.viewerDiv?.classList.remove("hidden");
                    this.viewerContainer?.classList.remove("bothViewer");
                    if (this.mapdiv)
                        this.viewerContainer?.removeChild(this.mapdiv);
                    this.mapdiv = null;
                    this.map = null;
                    break;
                case 1:
                    this.viewerContainer?.classList.add("bothViewer");
                    this.mapdiv = document.createElement("div");
                    this.viewerContainer?.append(this.mapdiv);
                    this.mapdiv.id = "map";
                    this.map = L.map('map').setView([this.locationVal.latitude, this.locationVal.longitude], 13);
                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        maxZoom: 19,
                        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                    }).addTo(this.map);
                    this.marker = L.marker([this.locationVal.latitude, this.locationVal.longitude]).addTo(this.map);
                    break;
                case 2:
                    this.viewerDiv?.classList.add("hidden");
                    if (this.map)
                        this.map.invalidateSize();
                    break;
            }
        }
    }
    image;
    locationVal = null;
    viewerContainer;
    viewerDiv;
    mapdiv = null;
    map = null;
    marker = null;
    viewMode = 0;
}
customElements.define('image-viewer', ImageViewer);
