import './components/virtualtablecomponent.js'
import { getProcessor } from './processors/processor.js'

class Folder extends HTMLElement {
    constructor() {
        super()
        this.folderId = this.getAttribute("id")
        const additionalStyle = ".exif {color: var(--exif-color);} .isSelected .exif {color: var(--selected-exif-color); }"
        this.innerHTML = `<virtual-table-component additionalStyle='${additionalStyle}'></virtual-table-component>`
        this.table = this.firstChild
        
        this.changePath()

        this.table.renderRow = (item, tr) => this.processor.renderRow(item, tr)
    }
    
    changeTheme(theme) {
        ["themeAdwaita", "themeAdwaitaDark"].forEach(n => this.table.classList.remove(n))
        this.table.classList.add(theme)    
        this.table.themeChanged()
    }

    showHidden(hidden) {
        this.showHiddenItems = hidden
        this.changePath(this.processor.getCurrentPath())
    }

    setFocus() { this.table.setFocus() }

    connectedCallback() {
        this.table.addEventListener("columnwidths", e => this.processor.saveWidths(e.detail))
        this.table.addEventListener("columnclick", e => {

            // [Log] columnclick – {column: 1, descending: false, subItem: false} (folder.js, line 33)
            // [Log] columnclick – {column: 1, descending: true, subItem: false} (folder.js, line 33)
            // [Log] columnclick – {column: 2, descending: false, subItem: false} (folder.js, line 33)
            // [Log] columnclick – {column: 0, descending: false, subItem: false} (folder.js, line 33)
            // [Log] columnclick – {column: 0, descending: false, subItem: true} (folder.js, line 33)
            // [Log] columnclick – {column: 0, descending: true, subItem: true} (folder.js, line 33)
            // [Log] columnclick – {column: 0, descending: false, subItem: true} (folder.js, line 33)
            //console.log("columnclick", e.detail)

            // TODO in processor
            // TODO reset when new items
            const ascDesc = sortResult => e.detail.descending ? -sortResult : sortResult
            let sortfn
            switch (e.detail.column) {
                case 1: 
                    sortfn = ([a, b]) => (a.exiftime ? a.exiftime : a.time) - (b.exiftime ? b.exiftime : b.time)
                    break
                case 2: 
                    sortfn = ([a, b]) => a.size - b.size
                    break
                default:
                    return
            }
            const sort = composeFunction(ascDesc, sortfn) 
            this.table.restrictClose()
            this.table.items = this.table.items.sort(sort)
            this.table.refresh()
        })

        this.table.addEventListener("enter", async evt => {
            const [path, recentFolder] = this.processor.getPath(this.table.items[evt.detail.currentItem])
            if (path) {
                await this.changePath(path)
                if (recentFolder) {
                    const index = this.table.items.findIndex(n => n.name == recentFolder)
                    this.table.setPosition(index)
                }
            }
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
                    this.table.items[pos].isSelected = !this.table.items[pos].isNotSelectable && !this.table.items[pos].isSelected 
                    this.table.setPosition(pos + 1)
                    break
                }
                case 107: { // Numlock +
                    this.table.items.forEach(n => n.isSelected = !n.isNotSelectable)
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

    async changePath(path) {
        const result = getProcessor(this.folderId, path, this.processor)
        let items = (await result.processor.getItems(path)).map(n => {
            n.isHidden = n.name && n.name[0] == '.' && n.name[1] != '.'
            return n
        })
        if (!this.showHiddenItems)
            items = items.filter(n => n.isHidden == false)
        if (!items)
            return


        this.table.setItems([])
        if (result.changed) {
            this.processor = result.processor
            const columns = this.processor.getColumns()
            this.table.setColumns(columns)
        }
    
        this.table.setItems(items)
        this.table.setRestriction((items, restrictValue) => 
            items.filter(n => n.name.toLowerCase()
                .startsWith(restrictValue.toLowerCase())
        ))
        if (await this.processor.addExtensions(items))
            this.table.refresh()
    }
}

customElements.define('folder-table', Folder)

// TODO Sorting
// TODO Restricting per sort table

// TODO path in edit field
// TODO path in SubTitle 
// TODO Save last path

// TODO History wich backspace and ctrl backspace
