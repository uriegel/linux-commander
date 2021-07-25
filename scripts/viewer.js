const viewerSplitter = document.getElementById('viewerSplitter')
const viewerImg = document.getElementById('viewerImg')
const viewerPdf = document.getElementById('viewerPdf')
const viewerVideo = document.getElementById('viewerVideo')

export function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => viewerSplitter.classList.remove(n))
    viewerSplitter.classList.add(theme)    
}

export function onShowViewer(show, path) {
    if (show == undefined)
        show = !viewerActive
    viewerActive = show
    viewerSplitter.setAttribute("secondInvisible", !show)
    if (show) {
        viewerVideo.classList.remove("hidden")
        viewerVideo.src = "file:///run/media/uwe/Video/Videos/Die Rechnung ging nicht auf.mkv"
        refresh(path)
    }
    else {
        viewerImg.src = null
        viewerVideo.src = null
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
    // const extPos = path.lastIndexOf(".")
    // const ext = extPos != -1 ? path.substr(extPos+1).toLowerCase() : ""
    // switch (ext) {
    //     case "png":
    //     case "jpg":
    //         viewerPdf.classList.add("hidden")
    //         viewerVideo.classList.add("hidden")
    //         viewerImg.classList.remove("hidden")
    //         viewerImg.src = `/commander/getview?path=${path}` 
    //         viewerVideo.src = null
    //         break
    //     case "pdf":
    //         viewerImg.classList.add("hidden")
    //         viewerVideo.classList.add("hidden")
    //         viewerPdf.classList.remove("hidden")
    //         viewerPdf.load(`/commander/getview?path=${path}`) 
    //         viewerVideo.src = null
    //         break
    //     case "mp3":
    //     case "mp4":
    //     case "mkv":
    //     case "wav":
    //         viewerPdf.classList.add("hidden")
    //         viewerImg.classList.add("hidden")
    //         viewerVideo.classList.remove("hidden")
    //         viewerVideo.src = `/commander/getvideo?path=${path}` 
    //         break
    //     default:
    //         viewerVideo.classList.add("hidden")
    //         viewerImg.classList.add("hidden")
    //         viewerPdf.classList.add("hidden")
    //         viewerVideo.src = null
    //         break
    // }
}

var viewerActive = false
var viewerRefresher = 0

