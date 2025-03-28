
var viewer = document.getElementById('viewer')
var viewerContainer = document.getElementById('viewerContainer')
var viewerDiv = document.getElementById('viewerDiv')

var previewMode = 0
var locationLatitude = null
var locationLongitude = null
var mapdiv
var map = null
var marker = null

registerSetPath(setPath)
registerToggleViewFunc(toggleView)

function setPath(path, lat, long) {
    locationLatitude = lat
    locationLongitude = long
    console.log("exifDataVal", path, locationLatitude, locationLongitude)
    const smallPath = path.toLowerCase();
    if (smallPath.endsWith(".jpg") || smallPath.endsWith(".jpeg") || smallPath.endsWith(".png"))
    {
        viewer.src = `/getfile?path=${path}`     
        viewer.classList.remove("hidden")
        if (mapdiv) {
            if (locationLatitude && locationLongitude) {
                mapdiv.classList.remove('hidden')  
                map.setView([locationLatitude, locationLongitude])
                if (marker)
                    marker.remove()
                marker = L.marker([locationLatitude, locationLongitude]).addTo(map)
            } else {
                mapdiv.classList.add('hidden')  
            }
        }
    }
    else
        viewer.classList.add("hidden")
}

function toggleView() {
    if (locationLatitude && locationLongitude) {
        previewMode++
        if (previewMode > 2)
            previewMode = 0

        switch (previewMode) {
            case 0:
                viewerDiv.classList.remove("hidden")
                viewerContainer.classList.remove("bothViewer")
                viewerContainer.removeChild(mapdiv)
                mapDiv = null
                map = null
                break
            case 1:
                viewerContainer.classList.add("bothViewer")
                mapdiv = document.createElement("div")
                viewerContainer.append(mapdiv)
                mapdiv.id = "map"
                map = L.map('map').setView([locationLatitude, locationLongitude], 13)
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                }).addTo(map);
                marker = L.marker([locationLatitude, locationLongitude]).addTo(map)
                break
            case 2:
                viewerDiv.classList.add("hidden")
                if (map)
                    map.invalidateSize()
                break
        }
    }
}