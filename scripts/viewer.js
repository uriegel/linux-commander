const viewerSplitter = document.getElementById('viewerSplitter')
const viewerImg = document.getElementById('viewerImg')

export function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => viewerSplitter.classList.remove(n))
    viewerSplitter.classList.add(theme)    
}

export function onShowViewer(show, path) {
    viewerActive = show
    viewerSplitter.setAttribute("secondInvisible", !show)
    refresh(path)
}

export const refreshViewer = path => {
    if (viewerActive) {
        if (viewerRefresher)
            clearTimeout(viewerRefresher)
        viewerRefresher = setTimeout(() => {
            viewerRefresher = 0
            refresh(path)
        }, 50)
    }
}

const refresh = path => {
    const extPos = path.lastIndexOf(".")
    const ext = extPos != -1 ? path.substr(extPos+1) : ""
    switch (ext) {
        case "png":
        case "jpg":
            viewerImg.classList.remove("hidden")
            break
        default:
            viewerImg.classList.add("hidden")
            break
    }
}

var viewerActive = false
var viewerRefresher = 0

