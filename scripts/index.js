import './components/gridsplitter.js'
import './folder.js'
const themeChooser = document.getElementById("themeChooser")

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
            changeTheme("themeAdwaita")
            break
        case 1: 
            changeTheme("themeAdwaitaDark")
            break
    }
}






