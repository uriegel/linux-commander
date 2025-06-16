export class TrackViewer extends HTMLElement {
    constructor() {
        super();
        const link = document.createElement("link");
        link.rel = "stylesheet";
        link.href = "../styles/trackviewer.css";
        this.appendChild(link);
        let template = document.getElementById("track-viewer");
        let templateContent = template.content;
        this.appendChild(templateContent.cloneNode(true));
    }
    set path(val) {
        this.setPath(val);
    }
    async setPath(path) {
        const track = await getTrack(path);
        this.trackPoints = track?.trackPoints?.map(n => [n.latitude, n.longitude]);
        if (this.trackPoints) {
            if (!this.map) {
                this.map = L.map('trackContainer').setView([0, 0], 13);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                }).addTo(this.map);
                const SliderControl = L.Control.extend({
                    onAdd: () => {
                        const container = L.DomUtil.create("div", "leaflet-bar leaflet-control");
                        container.classList.add("trackSlider");
                        this.slider = L.DomUtil.create("input", "", container);
                        this.slider.type = "range";
                        this.slider.min = '0';
                        this.slider.max = `${this.trackPoints?.length}`;
                        this.slider.value = '0';
                        // Prevent map zoom when interacting with slider
                        L.DomEvent.disableClickPropagation(container);
                        this.slider.addEventListener("input", () => {
                            let position = parseInt(this.slider.value);
                            this.marker?.setLatLng([this.trackPoints[position][0], this.trackPoints[position][1]]);
                            if (track?.trackPoints) {
                                this.addValue(this.trackValues, ".speed", track.trackPoints[position].velocity?.toFixed(1));
                                this.addValue(this.trackValues, ".heartRate", track.trackPoints[position].heartrate);
                            }
                        });
                        return container;
                    },
                });
                this.map.addControl(new SliderControl({ position: "bottomleft" }));
                const TableControl = L.Control.extend({
                    onAdd: () => {
                        const container = L.DomUtil.create("div", "leaflet-bar leaflet-control");
                        container.classList.add("trackTable");
                        container.classList.add("trackStatistics");
                        const template = document.getElementById("track-statistics");
                        container.appendChild(template.content.cloneNode(true));
                        this.statistics = container.querySelector("table");
                        return container;
                    },
                });
                this.map.addControl(new TableControl({ position: "topright" }));
                const TrackValesControl = L.Control.extend({
                    onAdd: () => {
                        const container = L.DomUtil.create("div", "leaflet-bar leaflet-control");
                        container.classList.add("trackTable");
                        container.classList.add("trackValues");
                        const template = document.getElementById("track-values");
                        container.appendChild(template.content.cloneNode(true));
                        this.trackValues = container.querySelector("table");
                        return container;
                    },
                });
                this.map.addControl(new TrackValesControl({ position: "bottomright" }));
            }
            this.slider.value = '0';
            this.slider.max = `${this.trackPoints?.length}`;
            const maxLat = this.trackPoints.reduce((prev, curr) => Math.max(prev, curr[0]), this.trackPoints[0][0]);
            const minLat = this.trackPoints.reduce((prev, curr) => Math.min(prev, curr[0]), this.trackPoints[0][0]);
            const maxLng = this.trackPoints.reduce((prev, curr) => Math.max(prev, curr[1]), this.trackPoints[0][1]);
            const minLng = this.trackPoints.reduce((prev, curr) => Math.min(prev, curr[1]), this.trackPoints[0][1]);
            this.map?.fitBounds([[maxLat, maxLng], [minLat, minLng]]);
            if (this.track)
                this.track.remove();
            this.track = L.polyline(this.trackPoints).addTo(this.map);
            if (this.marker)
                this.marker.remove();
            this.marker = L.marker([this.trackPoints[0][0], this.trackPoints[0][1]], { autoPan: true }).addTo(this.map);
            const v1 = this.addValue(this.statistics, ".dist", track?.distance.toFixed(1));
            const v2 = this.addValue(this.statistics, ".duration", this.formatDuration(track?.duration));
            const v3 = this.addValue(this.statistics, ".averageSpeed", track?.averageSpeed.toFixed(1));
            const v4 = this.addValue(this.statistics, ".maxSpeed", track?.maxSpeed.toFixed(1));
            const v5 = this.addValue(this.statistics, ".averageHeartRate", track?.averageHeartRate);
            const v6 = this.addValue(this.statistics, ".maxHeartRate", track?.maxHeartRate);
            if (v1 || v2 || v3 || v4 || v5 || v6)
                document.querySelector(".trackStatistics")?.classList.remove("hidden");
            else
                document.querySelector(".trackStatistics")?.classList.add("hidden");
            this.addValue(this.trackValues, ".speed", undefined);
            this.addValue(this.trackValues, ".heartRate", undefined);
        }
    }
    addValue(table, cellClass, val) {
        if (val) {
            const span = table?.querySelector(`${cellClass} span`);
            span.innerText = `${val}`;
            table?.querySelector(cellClass)?.classList.remove("hidden");
            return true;
        }
        else {
            table?.querySelector(cellClass)?.classList.add("hidden");
            return false;
        }
    }
    formatDuration(duration) {
        return duration
            ? duration / 3600 > 0
                ? `${Math.floor(duration / 3600).toString().padStart(2, '0')}:${Math.floor((duration % 3600) / 60).toString().padStart(2, '0')}`
                : `00:${Math.floor(duration / 60).toString().padStart(2, '0')}`
            : undefined;
    }
    map = null;
    track = null;
    trackPoints;
    marker = null;
    slider;
    statistics;
    trackValues;
}
async function getTrack(path) {
    try {
        const response = await fetch(`/gettrack?path=${path}`);
        return await response.json();
    }
    catch (e) {
        return null;
    }
}
customElements.define('track-viewer', TrackViewer);
