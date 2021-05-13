const template = document.createElement('template')
template.innerHTML = `  
    <style>
        :host {
            --vtc-color: black;
            --vtc-background-color: white;
            --vtc-caption-color: white;
            --vtc-selected-background-color: blue;
            --vtc-caption-background-color: blue;
            --vtc-caption-background-hover-color: #0063ff;
            --vtc-caption-separator-color: white;
            --vtc-selected-color:  white;
            --vtc-selected-background-color: blue;
            --vtc-current-color: lightgray;
            --vtc-current-focus-color: red;
        
            --vtc-font-size: 100%;
            --vtc-scrollbar-width: 16px;
            --vtc-scrollbar-border-color: gray;
            --vtc-scrollbar-border-width: 1px;
            --vtc-scrollbar-background-color: white;
            --vtc-scrollbar-button-background-color: white;
            --vtc-scrollbar-button-color: #666;
            --vtc-scrollbar-button-hover-color: #555
            --vtc-scrollbar-button-active-color: #444
            --vtc-scrollbar-button-hover-background-color: rgb(209, 209, 209);
            --vtc-scrollbar-button-active-background-color: #aaa;
            --vtc-scrollbar-grip-color: rgb(209, 209, 209); 
            --vtc-scrollbar-grip-hover-color: #bbb;
            --vtc-scrollbar-grip-active-color: #999;
            --vtc-scrollbar-grip-right: 0px;
            --vtc-scrollbar-grip-width: calc(100% - var(--vtc-scrollbar-grip-right));
            --vtc-scrollbar-right-margin: 15px;
        }
        .tableroot {
            position: absolute;
            height: 100%;
            overflow: hidden;
            background-color: var(--vtc-background-color);
            outline-width: 0px;
        }        
        table {
            width: 100%;
            border-spacing: 0px;
            color: var(--vtc-color);
            font-size: var(--vtc-font-size);
            table-layout: fixed;
        }
        table td {
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            user-select: none;
            transition: padding-right .4s;
        }            
        thead { 
            color: var(--vtc-caption-color);
            background-color: var(--vtc-caption-background-color);
        }
        th {
            text-overflow: ellipsis;
            user-select: none;            
            text-align: left;
            font-weight: normal;
            border-left-style: solid;
            border-left-width: 1px;
            border-left-color: var(--vtc-caption-separator-color);
            padding-left: 5px;
            padding-right: 5px;
            overflow: hidden;
            white-space: nowrap;
            -webkit-user-select: none;            
            user-select: none;            
        }
        th:first-child {
            border-left-width: 0px;
        }
        .tableroot.scrollbarActive td:last-child {
            padding-right: calc(3px + var(--vtc-scrollbar-right-margin));
        }
        .isCurrent {
            outline-color: var(--vtc-current-color);
            outline-width: 1px;
            outline-style: solid;
            outline-offset: -1px;
        }
        .tableroot:focus .isCurrent {
            outline-color: var(--vtc-current-focus-color);
        }
        .rightAligned {
            text-align: right;  
        }
        .isSortable {
            transition: background-color 0.3s; 
        }
        .isSortable:hover {
            background-color: var(--vtc-caption-background-hover-color);
        }        
        .sortAscending:before {
            position: relative;
            bottom: 11px;
            border-left: 4px solid transparent;
            border-right: 4px solid transparent;
            border-bottom: 6px solid var(--vtc-caption-color);
            content: '';
            margin-right: 5px;
        }
        .sortDescending:before {
            position: relative;
            top: 10px;
            border-left: 4px solid transparent;
            border-right: 4px solid transparent;
            border-top: 6px solid var(--vtc-caption-color);
            content: '';
            margin-right: 5px;
        }
        .scrollbar {
            position: absolute;
            width: var(--vtc-scrollbar-width); 
            right: 0px;
            overflow: hidden;
            border-style: solid;
            box-sizing: border-box;
            border-color: var(--vtc-scrollbar-border-color);
            border-width: var(--vtc-scrollbar-border-width);
            user-select: none;
            display: flex;
            flex-direction: column;  
            transition: transform 0.3s;  
            transform-origin: right top;
            bottom: 0px;
        }
        .scrollbar.hidden {
            transform: scale(0)
        }
        .svg {
            display: var(--vtc-scrollbar-button-display);
            width: 100%;
            background-color: var(--vtc-scrollbar-button-background-color);
            transition: background-color 0.3s;
        }
        .svg:hover {
            background-color: var(--vtc-scrollbar-button-hover-background-color);
        }
        .svg:active {
            background-color: var(--vtc-scrollbar-button-active-background-color);
            cursor: default;
        }
        .button {
            fill: var(--vtc-scrollbar-button-color);
            fill-opacity: 1; 
            stroke:none;            
        }
        .scrollbarElement {
            background-color: var(--vtc-scrollbar-background-color);
            flex-grow: 1;
            position: relative;	
        }
        .svg:hover .button {
            fill: var(--vtc-scrollbar-button-hover-color); 
        }
        .svg:active .button {
            fill: var(--vtc-scrollbar-button-active-color); 
        }        
        .grip {
            position: absolute;
            box-sizing: border-box;
            border-radius: var(--vtc-scrollbar-grip-radius);
            background-color: var(--vtc-scrollbar-grip-color);
            width: var(--vtc-scrollbar-grip-width);
            right: var(--vtc-scrollbar-grip-right);
            transition: background-color 0.5s, width 0.5s;
        }   
        .scrollbar:hover .grip {
            width: calc(100% - var(--vtc-scrollbar-grip-right));
        }
        .scrollbar:active .grip {
            width: calc(100% - var(--vtc-scrollbar-grip-right));
        }
        .grip:hover {
            background-color: var(--vtc-scrollbar-grip-hover-color);
        }
        .grip:active {
            background-color: var(--vtc-scrollbar-grip-active-color);
            transition: background-color 0s;
        }             
        .svgImage {
            width: 16px;
            height: 16px;
            vertical-align: bottom;
            margin-right: 3px;	
        }
        .svgImagePath {
            fill: var(--vtc-item-img-color);   
        }
        .isSelected {
            color: var(--vtc-selected-color);
            background-color: var(--vtc-selected-background-color);
        }
        .isSelected .svgImagePath {
            fill: var(--vtc-selected-color);   
        }
        #restrictionInput {
            width: 70%;
            bottom: 10px;
            height: 18px; 
            position: absolute;
            left: 5px;
            box-sizing: border-box;
            border-width: 1px;
            border-radius: 5px;
            padding: 1px 3px;
            border-style: solid;
            border-color: gray;
            color: var(--vtc-color);
            background-color: var(--vtc-background-color);
            box-shadow: 3px 5px 12px 3px rgba(136, 136, 136, 0.55);    
            transition: opacity 0.5s, width 0.5s;
        }
        #restrictionInput.invisible {
            opacity: 0;
            width: 0px;
        }
        #restrictionInput.none {
            display: none;
        }
    </style>
    <div class="tableroot" tabIndex=1>
        <table>
            <thead>
                <tr></tr>
            </thead>
            <tbody></tbody>
        </table>
        <input id="restrictionInput" class="invisible none" >
    </div>
    <div class="scrollbar hidden">
        <svg class="svg" viewBox="0 0 100 100" >
            <path class="button" d="M 20,70 50,30 80,70 Z" / >
        </svg>
        <div class="scrollbarElement">
            <div class="grip"></div>
        </div>
        <svg class="svg" viewBox="0 0 100 100" >
            <path class="button" d="M 80,30 50,70 20,30 Z" />
        </svg>
    </div>
` 

/**
 * @typedef {Object} Column
 * @property {string} title Title of column
 */

 const mouseRepeat = action => {
    action()
    let interval = 0
    const timeout = setTimeout(() => interval = setInterval(() => action(), 50), 600)
    const mouseUp = () => {
        window.removeEventListener("mouseup", mouseUp)
        clearTimeout(timeout)
        if (interval)
            clearInterval(interval)
    }
    window.addEventListener("mouseup", mouseUp)
}

class VirtualTableComponent extends HTMLElement {
    constructor() {
        super()
        this.items = []
        this.scrollPosition = 0
        this.attachShadow({ mode: 'open'})
        this.shadowRoot.appendChild(template.content.cloneNode(true))
        this.wheelTimestamp = performance.now()

        this.tableroot = this.shadowRoot.querySelector('.tableroot')
        this.headRow = this.shadowRoot.querySelector('thead>tr')
        this.tableBody = this.shadowRoot.querySelector('tbody')
        this.scrollbar = this.shadowRoot.querySelector('.scrollbar')
        this.scrollbarElement = this.shadowRoot.querySelector('.scrollbarElement')
        this.scrollbarGrip = this.shadowRoot.querySelector('.scrollbarElement>div')
        
        const buttons = this.shadowRoot.querySelectorAll('svg')
        this.upButton = buttons[0]
        this.downButton = buttons[1]

        this.restrictionInput = this.shadowRoot.getElementById("restrictionInput")
    }

    connectedCallback() {
       
        const onMouseMove = evt => {
            const element = evt.target.tagName == "TH" ? evt.target : evt.target.parentElement.parentElement
            const thWidth = element.clientWidth + element.clientLeft
            const mouseX = evt.offsetX + element.clientLeft
            const trRect = element.parentElement.getBoundingClientRect()
            const absoluteRight = trRect.width + trRect.x                
            let draggingReady = 
                (mouseX < 3 || mouseX > thWidth - 4) 
                && (evt.pageX - trRect.x > 4)
                && (evt.pageX < absoluteRight - 4)
            if (draggingReady && evt.target.tagName != "TH") {
                const first = evt.target.style.flexGrow == 1
                if (first && mouseX > thWidth - 4 || !first && mouseX < 3)
                    draggingReady = false
            }
            this.draggingReady = draggingReady
            document.body.style.cursor = this.draggingReady ? 'ew-resize' : 'auto'
        }

        const onMouseDown = evt => {
            if (this.draggingReady) {
                const th = evt.target
                const mouseX = evt.offsetX + th.clientLeft
                const dragleft = mouseX < 3
    
                const startDragPosition = evt.pageX
                const targetColumn = th.closest("th")
    
                const currentHeader = dragleft ? targetColumn.previousElementSibling : targetColumn
                if (!currentHeader)
                    return
                const nextHeader = currentHeader.nextElementSibling
                if (!nextHeader)
                    return
    
                const currentLeftWidth = currentHeader.offsetWidth
                const sumWidth = currentLeftWidth + nextHeader.offsetWidth
    
                const onmove = evt => {
                    document.body.style.cursor = 'ew-resize'
                    let diff = evt.pageX - startDragPosition
                    if (currentLeftWidth + diff < 15)
                        diff = 15 - currentLeftWidth
                    else if (diff > sumWidth - currentLeftWidth - 15)
                        diff = sumWidth - currentLeftWidth - 15
    
                    const getCombinedWidth = (column, nextColumn) => {
                        const firstWidth = 
                            column.style.width
                            ? parseFloat(column.style.width.substr(0, column.style.width.length - 1))
                            : 100 / this.columns.length
                        const secondWidth = 
                            nextColumn.style.width
                            ? parseFloat(nextColumn.style.width.substr(0, nextColumn.style.width.length - 1))
                            : 100 / this.columns.length
                        return firstWidth + secondWidth
                    }                        
    
                    const combinedWidth = getCombinedWidth(currentHeader, nextHeader)
    
                    let leftWidth = currentLeftWidth + diff
                    let rightWidth = sumWidth - currentLeftWidth - diff
                    const factor = combinedWidth / sumWidth
                    leftWidth = leftWidth * factor
                    rightWidth = rightWidth * factor
    
                    currentHeader.style.width = leftWidth + '%'
                    nextHeader.style.width = rightWidth + '%'
                    evt.preventDefault()
                }
    
                const onup = evt => {
                    const getWidths = () => {
                        const ths = Array.from(targetColumn.parentElement.children)
                         return ths.map(th => 
                             th.style.width 
                                ? parseFloat(th.style.width.substr(0, th.style.width.length - 1))
                                : 100 / this.columns.length
                         )
                    }
    
                    window.removeEventListener('mousemove', onmove)
                    window.removeEventListener('mouseup', onup)
                    document.body.style.cursor = 'auto'
                    
                    this.dispatchEvent(new CustomEvent('columnwidths', { detail: getWidths() }))
                    this.setFocus()
                    evt.preventDefault()
                    evt.stopPropagation()
                }
    
                window.addEventListener('mousemove', onmove)
                window.addEventListener('mouseup', onup)
                evt.preventDefault()
                evt.stopPropagation()
            }
        }

        window.addEventListener('resize', () => {
            if (!this.resizeTimer)
                this.resizeTimer = setTimeout(() => {
                    this.resizeTimer = 0
                    const lastItemsPerPage = this.itemsPerPage
                    this.measureItemsPerPage()
                    if (lastItemsPerPage != this.itemsPerPage) {
                        if (this.scrollPosition > this.items.length - this.itemsPerPage) 
                            this.scrollPosition = Math.max(this.items.length - this.itemsPerPage, 0)
                        this.render()
                    }
                }, 50)
        })
        this.headRow.addEventListener('mousemove', onMouseMove)
        this.headRow.addEventListener('mouseleave', () => {
            this.draggingReady = false
            document.body.style.cursor = 'auto'
        })        
        this.headRow.addEventListener('mousedown', onMouseDown)

        this.upButton.onmousedown = () => mouseRepeat(() => {
            this.scrollPosition = Math.max(this.scrollPosition - 1, 0)
            this.render()
            setTimeout(() => this.setFocus())
        })
        this.downButton.onmousedown = () => mouseRepeat(() => {
            this.scrollPosition = Math.min(this.scrollPosition + 1, this.items.length - this.itemsPerPage || 0)
            this.render()
            setTimeout(() => this.setFocus())
        })

        this.scrollbarElement.onmousedown = evt => this.onPageMouseDown(evt)
        this.scrollbarGrip.onmousedown = evt => this.onGripMouseDown(evt)
        this.tableroot.onwheel = evt => this.onWheel(evt)
        this.tableroot.onkeydown = evt => this.onkeydown(evt)
        this.tableroot.onmousedown = evt => {
            const el = evt.target
            const tr = el.closest("tbody tr")
            if (tr) {
                const currentIndex = 
                    Array
                        .from(tr.parentElement.children)
                         .findIndex(n => n == tr)
                     + this.scrollPosition
                if (currentIndex != -1) {
                    this.position = currentIndex
                    this.setFocused()
                    setTimeout(() => this.setFocus())
                }
                    
            }		
        }

        this.tableroot.ondblclick = evt => {
            this.dispatchEvent(new CustomEvent('enter', { detail: { currentItem: this.position } }))
        }

        this.restrictionInput.addEventListener("ontransitionend", evt => {
            if (this.restrictionInput.classList.contains("invisible"))
                this.restrictionInput.classList.add("none")
        })
    }

    /**
     * 
     * @param {Column[]} columns 
     */
    setColumns(columns) {
        this.columns = columns

        let last
        while (last = this.headRow.lastChild) 
            this.headRow.removeChild(last)
    
        columns.forEach((n, i) => {
            const th = document.createElement('th')
            if (n.width)
                th.style.width = n.width + '%'
            if (n.isSortable) {
                th.onclick = evt => {
                    if (this.draggingReady)
                        return

                    const remove = element =>  {
                        element.classList.remove("sortDescending")
                        element.classList.remove("sortAscending")
                    }

                    const subItem = evt.target.tagName == "SPAN" && evt.target.style.flexGrow != 1
                    Array.from(this.headRow.children)
                        .filter( n => n != th)
                        .forEach(n => {
                            if (n.firstChild.firstChild) {
                                remove(n.firstChild.firstChild)
                                remove(n.firstChild.lastChild)
                            }
                            else
                                remove(n)
                        })
                    let descending = false
                    
                    let element = th.firstChild.firstChild 
                        ? subItem ? th.firstChild.lastChild : th.firstChild.firstChild 
                        : th
                    if (element.classList.contains("sortAscending")) {
                        element.classList.remove("sortAscending")
                        element.classList.add("sortDescending")
                        descending = true
                    } else {
                        element.classList.remove("sortDescending")
                        element.classList.add("sortAscending")
                    }
                    if (th.firstChild.firstChild) {
                        let element = subItem ? th.firstChild.firstChild : th.firstChild.lastChild
                        remove(element)
                    }

                    this.dispatchEvent(new CustomEvent('columnclick', { detail: { column: i, descending, subItem } }))
                }
            }
            if (n.isRightAligned)
                th.classList.add("rightAligned")
            if (!n.subItem) {
                th.innerHTML = n.name
                if (n.isSortable)
                    th.classList.add("isSortable") 
            }
            else {
                const thDiv = document.createElement('div')
                thDiv.style.display = "flex"
                const thContent = document.createElement('span')
                thContent.innerHTML = n.name
                thContent.style.flexGrow = 1
                const thSubContent = document.createElement('span')
                thSubContent.innerHTML = n.subItem.name
                if (n.isSortable) {
                    thContent.classList.add("isSortable") 
                    thSubContent.classList.add("isSortable") 
                }
                thDiv.appendChild(thContent)
                thDiv.appendChild(thSubContent)
                th.appendChild(thDiv)
            }
            this.headRow.appendChild(th)
            this.scrollbar.style.height = `calc(100% - ${this.headRow.clientHeight}px)` 
        })
    }

    setItems(items) {
        this.items = items
        if (!this.itemHeight) 
            this.measureItemHeight()
            this.measureItemsPerPage()
        this.position = 0
        this.render()    
    }

    setRestriction(restrictCallback) { this.restrictCallback = restrictCallback }

    themeChanged() {
        this.measureItemHeight()
        this.measureItemsPerPage()
        this.render()    
    }

    setFocus() { this.tableroot.focus() }

    refresh() { this.render() }

    getPosition() { return this.position }
    setPosition(position) {
        position = Math.max(0, position)
        position = Math.min(position, this.items.length)
        const delta = position - this.position
        this.adjustPosition(delta, true)
        this.render()
    }

    measureItemsPerPage() { this.itemsPerPage = Math.floor((this.tableroot.clientHeight - this.headRow.clientHeight) / this.itemHeight) }
    
    measureItemHeight() {
        if (this.items.length > 0) {
            const tr = document.createElement('tr')
            const td = document.createElement('td')
            this.columns[0].render(td, this.items[0])
            tr.appendChild(td)
            this.tableBody.appendChild(tr)
            this.itemHeight = tr.offsetHeight
            this.tableBody.removeChild(tr)
        }
    }

    onPageMouseDown(evt) {
        const offsetY = evt.offsetY
        const gripTop = this.scrollbarGrip.offsetTop
		const gripHeight = this.scrollbarGrip.clientHeight
        const range = Math.max(0, this.items.length - this.itemsPerPage) + 1
        const isUp = offsetY <= gripTop
        
		const action = () => {
            const gripTop = this.scrollbarGrip.offsetTop
            if (isUp && offsetY < gripTop || !isUp && offsetY > gripTop + gripHeight) {
                this.scrollPosition = isUp 
                    ? Math.max(this.scrollPosition - this.itemsPerPage + 1, 0)
                    : Math.min(this.scrollPosition + this.itemsPerPage - 1, range -1)
                this.render()
            }
        }
        mouseRepeat(action)
        setTimeout(() => this.setFocus())
    }

    onGripMouseDown(evt) {
		const gripTop = this.scrollbarGrip.offsetTop
        const gripHeight = this.scrollbarGrip.clientHeight
		const startPos = evt.y - gripTop
		const range = this.scrollbarElement.clientHeight - gripHeight
		const maxPosition = this.items.length - this.itemsPerPage
		const onmove = evt => {
            const newTime = performance.now()
            const diff = newTime - this.wheelTimestamp
            if (diff > 20) {
                this.wheelTimestamp = newTime
    			const delta = evt.y - startPos
	    		const factor = Math.min(1, (Math.max(0, delta * 1.0 / range)))
		    	this.scrollPosition = Math.floor(factor * maxPosition)
                this.render()
            }
			evt.preventDefault()
			evt.stopPropagation()
		}
		const onup = () => {
			window.removeEventListener('mousemove', onmove, true)
			window.removeEventListener('mouseup', onup, true)
		}
		window.addEventListener('mousemove', onmove, true)
		window.addEventListener('mouseup', onup, true)

        this.setFocus()

        evt.preventDefault()
        evt.stopPropagation()
    }

    onWheel(evt) {
		if (this.items.length > this.itemsPerPage) {
            const newTime = performance.now()
            const diff = newTime - this.wheelTimestamp
            if (diff > 20) {
                this.wheelTimestamp = newTime
    
                var delta = evt.deltaY / Math.abs(evt.deltaY) * 3
                let newPos = this.scrollPosition + delta
                if (newPos < 0)
                    newPos = 0
                if (newPos > this.items.length - this.itemsPerPage) 
                    newPos = this.items.length - this.itemsPerPage 
                this.scrollPosition = newPos
                this.render()
            }
		}        
    }

    onkeydown(evt) {

        const checkRestriction = () => {
            if (!evt.altKey && !evt.ctrlKey && evt.key.length ==1) {
                if (this.restrictTo(evt.key)) {
                    evt.preventDefault()
                    evt.stopPropagation()
                    return true
                }
            }
            return false
        }

        const restrictClose = () => {
            if (this.restriction) {
                this.items = this.restriction.originalItems
                this.setPosition(0)
                this.render()
                this.restrictionInput.classList.add("invisible")
                this.restriction = null
                evt.preventDefault()
                evt.stopPropagation()
            }
        }

        const restrictBack = () => {
            if (this.restriction) {
                this.restrictionInput.value = this.restrictionInput.value.substr(0, this.restrictionInput.value.length - 1)
                if (!this.restrictionInput.value)
                    restrictClose()
                else {
                    this.items = this.restrictCallback(this.restriction.originalItems, this.restrictionInput.value)
                    this.setPosition(0)
                    this.render()
                    evt.preventDefault()
                    evt.stopPropagation()
                }
            }
        }

        let delta
        switch (evt.which) {
            case 8: // backspace
                restrictBack()
                return
            case 13: // enter
                this.dispatchEvent(new CustomEvent('enter', { detail: { currentItem: this.position } }))
                return
            case 27: // esc
                restrictClose()
                return
            case 33: // pageUp
                delta = -this.itemsPerPage + 1
                break     
            case 34: // pageDown
                delta = this.itemsPerPage - 1
                break     
            case 35: // end
                if (evt.shiftKey)
                    return
                delta = this.items.length - 1 - this.position
                break
            case 36: // home
                if (evt.shiftKey)
                    return
                delta = -this.position
                break
            case 40: // down
                delta = 1
                break
            case 38: // up
                delta = -1
                break
            default:
                checkRestriction()
                return
        }
        this.adjustPosition(delta, true)
        this.render()
}

    adjustPosition(delta, scrollIntoView) {
        this.position = delta > 0 
            ? Math.min(this.position + delta, this.items.length - 1)
            : Math.max(this.position + delta, 0)
        if (scrollIntoView) {
            const down = delta > 0
            this.scrollPosition += down 
                ? Math.max(0, this.position - this.scrollPosition - this.itemsPerPage + 1)
                : - Math.max(0, this.scrollPosition - this.position)
            if (down && this.position - this.scrollPosition < 0)
                this.scrollPosition = this.position
            if (!down && this.position - this.scrollPosition - this.itemsPerPage + 1 >= 0)
                this.scrollPosition = this.position - this.itemsPerPage + 1
        }
    }

    setFocused() {
        Array
            .from(this.tableBody.children)
            .forEach(n => n.classList.remove("isCurrent"))
        let index = this.position - this.scrollPosition
        if (index > 0 && index < this.items.length) 
            this.tableBody.children[index].classList.add("isCurrent")
    }

    render() {
        this.renderItems()
        this.renderScrollbarGrip()
    }

    renderRow = (item, tr) => {}
    
    renderItems() {
        let last
        while (last = this.tableBody.lastChild) 
            this.tableBody.removeChild(last)

        for (let i = this.scrollPosition; 
                i < Math.min(this.itemsPerPage + 1 + this.scrollPosition, this.items.length);
                i++) {
            const tr = this.renderItem(this.items[i], i)
            this.tableBody.appendChild(tr)
        }
    }

    renderItem(item, index) {
        const tr = document.createElement('tr')
        this.renderRow(item, tr)
        this.columns.forEach(col => {
            const td = document.createElement('td')
            if (col.isRightAligned)
                td.classList.add("rightAligned")
            td.classList.add()
            col.render(td, item)
            tr.appendChild(td)
        }) 
        if (this.position == index) 
            tr.classList.add("isCurrent")
        if (item.isSelected) 
            tr.classList.add("isSelected")
        return tr
    }

    renderScrollbarGrip() {
        const range = Math.max(0, this.items.length - this.itemsPerPage) + 1
        const gripHeight = Math.max(this.scrollbarElement.clientHeight * (this.itemsPerPage / this.items.length || 1), 10)
        this.scrollbarGrip.style.top = (this.scrollbarElement.clientHeight - gripHeight) * (this.scrollPosition / (range -1))
        this.scrollbarGrip.style.height = `${gripHeight}px`
        if (this.itemsPerPage > this.items.length - 1) {
            this.scrollbar.classList.add('hidden')
            this.tableroot.classList.remove('scrollbarActive')
        }
        else {
            this.scrollbar.classList.remove('hidden')
            this.tableroot.classList.add('scrollbarActive')
        }
    }

    restrictTo(newValue) {
        if (!this.restriction) {
            const restrictedItems = this.restrictCallback(this.items, newValue)
            if (restrictedItems && restrictedItems.length > 0) {
                this.restriction = { originalItems: this.items }
                this.restrictionInput.classList.remove("none")
                setTimeout(() => this.restrictionInput.classList.remove("invisible"))
                this.restrictionInput.value = newValue
                this.items = restrictedItems
                this.setPosition(0)
                this.render()
                return true
            }
        } else {
            const restrictedItems = this.restrictCallback(this.items, this.restrictionInput.value + newValue)
            if (restrictedItems.length > 0) {
                this.restrictionInput.value += newValue
                this.items = restrictedItems
                this.setPosition(0)
                this.render()
                return true
            }
        }
        return false
    }
}

customElements.define('virtual-table-component', VirtualTableComponent)