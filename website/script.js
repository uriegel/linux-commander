
var viewer = document.getElementById('viewerImg')

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