
var viewer = document.getElementById('viewer')
var viewerContainer = document.getElementById('viewerContainer')

var previewMode = 0

function setPath(path) {
    const smallPath = path.toLowerCase();
    if (smallPath.endsWith(".jpg") || smallPath.endsWith(".jpeg") || smallPath.endsWith(".png"))
    {
        viewer.src = `/getfile?path=${path}`     
        viewer.classList.remove("hidden")
    }
    else
        viewer.classList.add("hidden")
}

var mapdiv

function togglePreview() {
    previewMode++
    if (previewMode == 3)
        previewMode = 0
    
    if (previewMode == 1) {
        viewerContainer.classList.add("bothViewer")
        mapdiv = document.createElement("div")
        viewerContainer.append(mapdiv)
        mapdiv.id = "map"
        const map = L.map('map').setView([51.505, -0.09], 13)
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);
        var marker = L.marker([51.5, -0.09]).addTo(map)
    }
    else {
        viewerContainer.classList.remove("bothViewer")
        viewerContainer.removeChild(mapdiv)
    }

}