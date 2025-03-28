
var viewer = document.getElementById('viewer')
var viewerContainer = document.getElementById('viewerContainer')

var previewMode = 0
var locationLatitude = null
var locationLongitude = null
var mapdiv

function setPath(path, lat, long) {
    locationLatitude = lat
    locationLongitude = long
    console.log("exifDataVal", path, locationLatitude, locationLongitude)
    const smallPath = path.toLowerCase();
    if (smallPath.endsWith(".jpg") || smallPath.endsWith(".jpeg") || smallPath.endsWith(".png"))
    {
        viewer.src = `/getfile?path=${path}`     
        viewer.classList.remove("hidden")
    }
    else
        viewer.classList.add("hidden")
}

function togglePreview() {
    if (locationLatitude && locationLongitude) {
        previewMode++
        if (previewMode == 3)
            previewMode = 0
        
        if (previewMode == 1) {
            viewerContainer.classList.add("bothViewer")
            mapdiv = document.createElement("div")
            viewerContainer.append(mapdiv)
            mapdiv.id = "map"
            const map = L.map('map').setView([locationLatitude, locationLongitude], 13)
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }).addTo(map);
            var marker = L.marker([locationLatitude, locationLongitude]).addTo(map)
        }
        else {
            viewerContainer.classList.remove("bothViewer")
            viewerContainer.removeChild(mapdiv)
        }
    }
}