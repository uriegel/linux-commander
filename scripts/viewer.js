const viewerSplitter = document.getElementById('viewerSplitter')
const viewerImg = document.getElementById('viewerImg')
const viewerFrame = document.getElementById('viewerFrame')

export function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => viewerSplitter.classList.remove(n))
    viewerSplitter.classList.add(theme)    
}

export function onShowViewer(show, path) {
    viewerActive = show
    viewerSplitter.setAttribute("secondInvisible", !show)
    if (show) 
        refresh(path)
    else {
        viewerImg.src = null
        viewerFrame.src = null
    }
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
    const ext = extPos != -1 ? path.substr(extPos+1).toLowerCase() : ""
    switch (ext) {
        case "png":
        case "jpg":
            viewerFrame.classList.add("hidden")
            viewerImg.classList.remove("hidden")
            viewerImg.src = `/commander/getview?path=${path}` 
            break
        case "pdf":
            viewerImg.classList.add("hidden")
            viewerFrame.classList.remove("hidden")
            viewerFrame.src = `/commander/getview?path=${path}` 
            break
        default:
            viewerImg.classList.add("hidden")
            viewerFrame.classList.add("hidden")
            break
    }
}

var viewerActive = false
var viewerRefresher = 0

