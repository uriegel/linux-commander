const viewerSplitter = document.getElementById('viewerSplitter')
const viewerImg = document.getElementById('viewerImg')
const viewerPdf = document.getElementById('viewerPdf')
const viewerVideo = document.getElementById('viewerVideo')

export function onShowViewer(show, path) {
    if (show == undefined)
        show = !viewerActive
    viewerActive = show
    viewerSplitter.setAttribute("secondInvisible", !show)
    if (show) 
        refresh(path)
    else {
        viewerImg.src = undefined
        viewerVideo.src = undefined
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
            viewerPdf.classList.add("hidden")
            viewerVideo.classList.add("hidden")
            viewerImg.classList.remove("hidden")
            viewerImg.src = `commander/getfile?file=${path}` 
            viewerVideo.src = undefined
            break
        case "pdf":
            viewerImg.classList.add("hidden")
            viewerVideo.classList.add("hidden")
            viewerPdf.classList.remove("hidden")
            viewerPdf.load(`commander/getfile?file=${path}`) 
            viewerVideo.src = undefined
            break
        case "mp3":
        case "mp4":
        case "mkv":
        case "wav":
            viewerPdf.classList.add("hidden")
            viewerImg.classList.add("hidden")
            viewerVideo.classList.remove("hidden")
            viewerVideo.src = `commander/getfile?file=${path}` 
            break
        default:
            viewerVideo.classList.add("hidden")
            viewerImg.classList.add("hidden")
            viewerPdf.classList.add("hidden")
            viewerVideo.src = undefined
            break
    }
}

var viewerActive = false
var viewerRefresher = 0

