import './components/virtualtablecomponent.js'
import { getProcessor } from './processors/processor.js'

class Folder extends HTMLElement {
    constructor() {
        super()
        this.folderId = this.getAttribute("id")
        const additionalStyle = ".exif {color: var(--exif-color);} .isSelected .exif {color: var(--selected-exif-color); }"
        this.innerHTML = `
            <div class=folder>
                <input class=pathInput></input>
                <virtual-table-component additionalStyle='${additionalStyle}'></virtual-table-component>
            </div`
        
        this.table = this.getElementsByTagName("VIRTUAL-TABLE-COMPONENT")[0]
        this.backtrack = []
        this.backPosition = -1
        this.pathInput = this.getElementsByTagName("INPUT")[0]
        this.table.renderRow = (item, tr) => this.processor.renderRow(item, tr)
        const lastPath = localStorage.getItem(`${this.folderId}-lastPath`)
        initializeFolderEvents(this.folderId, msg => setTimeout(() => this.onEvent(msg))) 
        this.changePath(lastPath)
    }

    get id() { return this.folderId}
    
    changeTheme(theme) {
        ["themeAdwaita", "themeAdwaitaDark"].forEach(n => {
            this.table.classList.remove(n)
            this.pathInput.classList.remove(n)
        })
        this.table.classList.add(theme)    
        this.pathInput.classList.add(theme)    
        this.table.themeChanged()
    }

    onResize() {
        this.table.themeChanged()
    }

    showHidden(hidden) {
        this.showHiddenItems = hidden
        this.reloadItems()
    }

    reloadItems() {
        this.changePath(this.processor.getCurrentPath())
    }

    setFocus() { this.table.setFocus() }

    connectedCallback() {
        this.table.addEventListener("columnwidths", e => this.processor.saveWidths(e.detail))
        this.table.addEventListener("columnclick", e => {
            const sortfn = this.processor.getSortFunction(e.detail.column, e.detail.subItem)
            if (!sortfn)
                return
            const ascDesc = sortResult => e.detail.descending ? -sortResult : sortResult
            this.sortFunction = composeFunction(ascDesc, sortfn) 
            this.table.restrictClose()
            const dirs = this.table.items.filter(n => n.isDirectory)
            const files = this.table.items.filter(n => !n.isDirectory)
            this.table.items = dirs.concat(files.sort(this.sortFunction))
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

        this.table.addEventListener("delete", async evt => {
            const selectedItems = this.table.items
                .filter(n => n.isSelected) 
            if (selectedItems.length == 0 && evt.detail.currentItem == 0)
                return
            const param = selectedItems.length > 0 ? selectedItems : [ this.table.items[evt.detail.currentItem] ] 
            this.dispatchEvent(new CustomEvent('delete', { detail: param }))
        })
                
        this.table.addEventListener("keydown", evt => {
            switch (evt.which) {
                case 8: // backspace
                    this.getHistoryPath(evt.shiftKey)
                    return
                case 9: // tab
                    if (evt.shiftKey) {
                        this.pathInput.focus()
                    } else 
                        this.dispatchEvent(new CustomEvent('tab', { detail: this.id }))
                    evt.preventDefault()
                    evt.stopPropagation()
                    break
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
                case 82: { // "R"
                    if (evt.ctrlKey) 
                        this.reloadItems()
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

        this.table.addEventListener("focusin", async evt => {
            this.dispatchEvent(new CustomEvent('onFocus', { detail: this.id }))
            this.sendStatusInfo(this.table.getPosition())
        })

        this.table.addEventListener("currentIndexChanged", evt => this.sendStatusInfo(evt.detail))
            
        this.pathInput.onkeydown = evt => {
            if (evt.which == 13) {
                this.changePath(this.pathInput.value)
                this.table.setFocus()
            }
        }
        this.pathInput.onfocus = () => setTimeout(() => this.pathInput.select())
    }

    async changePath(path, fromBacklog) {
        const result = getProcessor(this.folderId, path, this.processor)
        let items = (await result.processor.getItems(this.folderId, path, this.showHiddenItems))
        if (!items) 
            return
            
        this.table.setItems([])
        if (result.changed) {
            this.processor = result.processor
            const columns = this.processor.getColumns()
            this.table.setColumns(columns)
            this.sortFunction = null
        }

        const dirs = items.filter(n => n.isDirectory)
        const files = items.filter(n => !n.isDirectory)
        this.dirsCount = dirs.length
        this.filesCount = files.length

        if (this.sortFunction) 
            items = dirs.concat(files.sort(this.sortFunction))

        this.table.setItems(items)
        this.table.setRestriction((items, restrictValue) => 
            items.filter(n => n.name.toLowerCase()
                .startsWith(restrictValue.toLowerCase())
        ))
        
        this.onPathChanged(path, fromBacklog)
    }

    onPathChanged(newPath, fromBacklog) {
        const path = newPath || this.processor.getCurrentPath()
        this.pathInput.value = path
        localStorage.setItem(`${this.folderId}-lastPath`, path)
        if (!fromBacklog) {
            this.backPosition++
            this.backtrack.length = this.backPosition
            this.backtrack.push(path)
        }
    }

    getCurrentPath() {
        return this.processor.getCurrentPath()
    }

    getHistoryPath(forward) {
        if (!forward && this.backPosition >= 0) {
            this.backPosition--
            this.changePath(this.backtrack[this.backPosition], true)
        } else if (forward && this.backPosition < this.backtrack.length - 1) {
            this.backPosition++
            this.changePath(this.backtrack[this.backPosition], true)
        }
    }

    onEvent(msg) {
        switch (msg.msgType) {
            case "ExifItem":
                if (this.processor.onEvent(this.table.items, msg.items))
                    this.table.refresh()
                break
            case "Refresh":
                this.reloadItems()
                break
        }
    }

    sendStatusInfo(index) {
        if (this.table.items && this.table.items.length > 0)
            this.dispatchEvent(new CustomEvent('pathChanged', { detail: {
                path: this.processor.getItem(this.table.items[index]),
                dirs: this.dirsCount,
                files: this.filesCount
            }}))
    }
}

customElements.define('folder-table', Folder)

// TODO Delete items: refresh
// TODO Delete items: show progress
// TODO Clean up main

// TODO Rename
// TODO CreateFolder
// TODO Copy
// TODO Copy: progress control in status bar
// TODO Move
// TODO Copy with Copy Paste (from external or from internal)
// TODO When a path is not available anymore: fallback to root
// TODO ProgressControl: multiple progresses: show in ProgressBars in popovermenu, show latest in ProgressPie
// TODO ProgressControl: On Error: Red X: Show Errors in List in popover

// TODO Status line (# files, # selected files), root
// TODO Status Linux: styling

// TODO xdg-open