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
        
        this.changePath()
    }
    
    changeTheme(theme) {
        ["themeAdwaita", "themeAdwaitaDark"].forEach(n => this.table.classList.remove(n))
        const style = getComputedStyle(document.body)
        exifColor = style.getPropertyValue('--exif-color') 
        selectedExifColor = style.getPropertyValue('--selected-exif-color') 
        this.table.classList.add(theme)    
        this.table.themeChanged()
    }

    setFocus() { this.table.setFocus() }

    connectedCallback() {
        this.table.addEventListener("columnwidths", e => this.processor.saveWidths(e.detail))
        this.table.addEventListener("columnclick", e => {
            console.log("columnclick", e.detail)
        })
        
        this.table.addEventListener("enter", evt => {
            const pathPart = this.table.items[evt.detail.currentItem].name
            this.changePath(pathPart, true)
        })
        
        this.table.addEventListener("keydown", evt => {
            switch (evt.which) {
                case 35: // end
                    if (evt.shiftKey) {
                        const pos = this.table.getPosition()
                        this.table.items.forEach((item, i) => item.isSelected = !item.isNotSelectable && i >= pos)                     
                        this.table.refresh()
                    }
                    break
                case 36: // home
                    if (evt.shiftKey) {
                        const pos = this.table.getPosition()
                        this.table.items.forEach((item, i) => item.isSelected = !item.isNotSelectable && i <= pos)                     
                        this.table.refresh()
                    }
                    break
                case 45: { // Ins
                    const pos = this.table.getPosition()
                    this.table.items[pos].isSelected = !this.table.items[pos].isSelected 
                    this.table.setPosition(pos + 1)
                    break
                }
                case 107: { // Numlock +
                    this.table.items.forEach(n => n.isSelected = true)
                    this.table.refresh()
                    break
                }
                case 109: { // Numlock -
                    this.table.items.forEach(n => n.isSelected = false)
                    this.table.refresh()
                    break
                }
            }
        })
    }

    async changePath(pathPart, partialPath) {
        const result = getProcessor(this.folderId, pathPart, partialPath, this.processor)
        this.processor = result.processor
        if (result.changed) {
            const columns = this.processor.getColumns()
            this.table.setColumns(columns)
        }
        const items = await this.processor.getItems("root")
        this.table.setItems(items)
        this.table.setRestriction((items, restrictValue) => 
            items.filter(n => n.name.toLowerCase()
                .startsWith(restrictValue.toLowerCase())
        ))
       
        // pub struct RootItem {
        //     pub name: String,
        //     pub display: String,
        //     pub mount_point: String,
        //     pub capacity: u64,
        //     pub file_system: String,
        // }

    }
}

customElements.define('folder-table', Folder)

// TODO: root items are selectable
// TODO: root items sorting: 1. with mountpoint, 2. without
// TODO: root items without mountpoint with opacity
// TODO: changePath: mountpoint or name? => rootItem: mountPoint is name, name is 