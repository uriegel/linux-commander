var setPathFunc
var toggleViewFunc

function registerSetPath(val) {
    setPathFunc = val
}
    
function registerToggleViewFunc(val) {
    toggleViewFunc = val
}

function setPath(path, lat, long) {
    setPathFunc(path, lat, long)
}
    
function toggleView() {
    toggleViewFunc()
}