import './components/gridsplitter.js'
import './folder.js'

const folderLeft = document.getElementById("folderLeft")
const folderRight = document.getElementById("folderRight")

const theme = initializeOnTheme(onTheme)
onTheme(theme)

function onTheme(theme) {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => document.body.classList.remove(n))
    document.body.classList.add(theme)    
    folderLeft.changeTheme(theme)
    folderRight.changeTheme(theme)
}

folderLeft.setFocus()










