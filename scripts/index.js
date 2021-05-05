import './components/gridsplitter.js'
import './folder.js'
const themeChooser = document.getElementById("themeChooser")

const folderLeft = document.getElementById("folderLeft")
const folderRight = document.getElementById("folderRight")

const changeTheme = theme => {
    ["themeAdwaita", "themeAdwaitaDark"].forEach(n => document.body.classList.remove(n))
    document.body.classList.add(theme)    
    folderLeft.changeTheme(theme)
    folderRight.changeTheme(theme)
}

themeChooser.onchange = () => {
    switch (themeChooser.selectedIndex) {
        case 0: 
            changeTheme("themeAdwaita")
            break
        case 1: 
            changeTheme("themeAdwaitaDark")
            break
    }
}

changeTheme("themeAdwaita")






