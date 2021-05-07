import './components/virtualtablecomponent.js'
import { getProcessor } from './processors/processor.js'

var exifColor = getComputedStyle(document.body).getPropertyValue('--exif-color') 
var selectedExifColor = getComputedStyle(document.body).getPropertyValue('--selected-exif-color') 

class Folder extends HTMLElement {
    constructor() {
        super()
        this.folderId = this.getAttribute("id")
        this.innerHTML = "<virtual-table-component></virtual-table-component>"
        this.table = this.firstChild
        
        const result = getProcessor(this.folderId)
        this.processor = result.processor
        const columns = this.processor.getColumns()
        this.table.setColumns(columns)

        
            
        const items = Array.from(Array(4000).keys())
            .map(index => ({
                name: "Eintrag " + index,
                ext: "ext",
                date: "24.03.1999 14:23",
                size: 2344 + index
            }))
        
        this.table.setItems(items)
        this.table.setRestriction((items, restrictValue) => items.filter(n => n.name.toLowerCase().startsWith(restrictValue.toLowerCase())))



        
    }
    
    changeTheme(theme) {
        ["themeAdwaita", "themeAdwaitaDark"].forEach(n => this.table.classList.remove(n))
        const style = getComputedStyle(document.body)
        exifColor = style.getPropertyValue('--exif-color') 
        selectedExifColor = style.getPropertyValue('--selected-exif-color') 
        this.table.classList.add(theme)    
        this.table.themeChanged()
    }

    connectedCallback() {
        this.table.addEventListener("columnwidths", e => this.processor.saveWidths(e.detail))
        this.table.addEventListener("columclick", e => {
            console.log("columclick", e.detail)
        })
        this.table.addEventListener("keydown", evt => {
            switch (evt.which) {
                case 35: // end
                    if (evt.shiftKey) {
                        const pos = table.getPosition()
                        table.items.forEach((item, i) => item.isSelected = !item.isNotSelectable && i >= pos)                     
                        table.refresh()
                    }
                    break
                case 36: // home
                    if (evt.shiftKey) {
                        const pos = table.getPosition()
                        table.items.forEach((item, i) => item.isSelected = !item.isNotSelectable && i <= pos)                     
                        table.refresh()
                    }
                    break
                case 45: { // Ins
                    const pos = table.getPosition()
                    table.items[pos].isSelected = !table.items[pos].isSelected 
                    table.setPosition(pos + 1)
                    break
                }
                case 107: { // Numlock +
                    table.items.forEach(n => n.isSelected = true)
                    table.refresh()
                    break
                }
                case 109: { // Numlock -
                    table.items.forEach(n => n.isSelected = false)
                    table.refresh()
                    break
                }
            }
        })
    }
}

customElements.define('folder-table', Folder)