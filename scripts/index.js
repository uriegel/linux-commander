import './components/gridsplitter.js'
import { VERTICAL, HORIZONTAL } from '/components/gridsplitter.js'

const themeChooser = document.getElementById("themeChooser")
const gridChooser = document.getElementById("gridChooser")
const secondInvisible = document.getElementById("secondInvisible")
const splitter = document.querySelector('grid-splitter')

themeChooser.onchange = () => {
    const changeTheme = theme => {
        ["themeBlue", "themeAdwaita", "themeAdwaitaDark"].forEach(n => {
            document.body.classList.remove(n)    
            table.classList.remove(n)    
        })
        document.body.classList.add(theme)    
        const style = getComputedStyle(document.body)
        exifColor = style.getPropertyValue('--exif-color') 
        selectedExifColor = style.getPropertyValue('--selected-exif-color') 
        table.classList.add(theme)    
        table.themeChanged()
    }

    switch (themeChooser.selectedIndex) {
        case 0: 
            changeTheme("themeBlue")
            break
        case 1: 
            changeTheme("themeAdwaita")
            break
        case 2: 
            changeTheme("themeAdwaitaDark")
            break
    }
}

gridChooser.onchange = () => {
    switch (gridChooser.selectedIndex) {
        case 0: 
            splitter.setAttribute("orientation", HORIZONTAL)
            break
        case 1: 
            splitter.setAttribute("orientation", VERTICAL)
            break
    }
}

secondInvisible.onchange = () => splitter.setAttribute("secondInvisible", secondInvisible.checked)