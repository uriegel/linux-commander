
var viewer = document.getElementById('viewerImg')

function setPath(path) {
    viewer.src = `/getfile?path=${path}` 
}