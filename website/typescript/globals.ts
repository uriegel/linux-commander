var setPathFunc: SetPathFunc
var toggleViewFunc: ToggleViewFunc

type SetPathFunc = (path: string, lat?: number, long?: number) => void
type ToggleViewFunc = () => void

function registerSetPath(val: SetPathFunc) {
    setPathFunc = val
}
    
function registerToggleViewFunc(val: ToggleViewFunc) {
    toggleViewFunc = val
}

function setPath(path: string, lat?: number, long?: number) {
    setPathFunc(path, lat, long)
}
    
function toggleView() {
    toggleViewFunc()
}