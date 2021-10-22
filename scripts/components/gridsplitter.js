const template = document.createElement('template')
template.innerHTML = `  
    <style>
        #splitterGrid {
            display:flex;
            flex-grow: 1;
            flex-direction: row;        
            width: 100%;
            height: 100%;
        }   
        .slot {
            flex-grow: 1;
            display: flex;
        }     
        #splitter {
            flex: 0 0 6px;
            cursor: ew-resize;
            transition: background-color 0.4s;
            background-color: var(--gridsplitter-grip-color);
        }
        #splitter:hover {
            background-color: var(--gridsplitter-grip-hover-color);
        }
        #splitter:active {
            background-color: var(--gridsplitter-grip-active-color);
        }        
        #splitterGrid.vertical {
            flex-direction: column;
        }
        .secondInvisible #second, .secondInvisible #splitter {
            display: none;
        }
        .vertical #splitter {
            cursor: ns-resize;
        }
    </style>
    <div id="splitterGrid">
        <div class="slot" id="first">
            <slot name="first"></slot>
        </div>
        <div id="splitter"></div>
        <div class="slot" id="second">
            <slot name="second"></slot>
        </div>
    </div>
` 

export var HORIZONTAL = "HORIZONTAL"
export var VERTICAL = "VERTICAL"

/**
 * @typedef {Object} Column
 * @property {string} title Title of column
 */

class GridSplitter extends HTMLElement {
    constructor() {
        super()

        var style = document.createElement("style")
        document.head.appendChild(style)
        style.sheet.insertRule(`:root {
            --gridsplitter-grip-color : gray;
            --gridsplitter-grip-hover-color : rgb(94, 94, 94);
            --gridsplitter-grip-active-color : rgb(61, 61, 61);
        }`)
    
        this.attachShadow({ mode: 'open' })
        this.shadowRoot.appendChild(template.content.cloneNode(true))
        this.splitterGrid = this.shadowRoot.getElementById("splitterGrid")
        this.splitter = this.shadowRoot.getElementById("splitter")
        this.first = this.shadowRoot.getElementById("first")
        this.second = this.shadowRoot.getElementById("second")
        if (this.attributes.orientation == "VERTICAL")
            this.splitterGrid.classList.add("vertical") 
    }

    connectedCallback() {
        this.splitter.addEventListener("mousedown", evt => {
            if (evt.which != 1) 
    			return
            const isVertical = this.getAttribute("orientation") == "VERTICAL"
		    const size1 = isVertical ? this.first.offsetHeight : this.first.offsetWidth
		    const size2 = isVertical ? this.second.offsetHeight : this.second.offsetWidth
		    const initialPosition = isVertical ? evt.pageY : evt.pageX		

            let timestap = performance.now()

            const onmousemove = evt => {

                const newTime = performance.now()
                const diff = newTime - timestap
                if (diff > 20) {
                    timestap = newTime

                    let delta = (isVertical ? evt.pageY : evt.pageX) - initialPosition
                    if (delta < 10 - size1)
                        delta = 10 - size1
                    if (delta > (isVertical ? this.first.parentElement.offsetHeight : this.first.parentElement.offsetWidth) - 10 - size1 - 6)
                        delta = (isVertical ? this.first.parentElement.offsetHeight : this.first.parentElement.offsetWidth) - 10 - size1 - 6

                    const newSize1 = size1 + delta
                    const newSize2 = size2 - delta

                    const procent2 = newSize2 / (newSize2 + newSize1 + 
                        (isVertical ? this.splitter.offsetHeight : this.splitter.offsetWidth)) * 100

                    const size = `0 0 ${procent2}%` 
                    this.second.style.flex = size
                    // if (positionChanged)
                    // positionChanged()
                }
                evt.stopPropagation()
                evt.preventDefault()
            }

            const onmouseup = evt => {
                window.removeEventListener('mousemove', onmousemove, true)
                window.removeEventListener('mouseup', onmouseup, true)

                evt.stopPropagation()
                evt.preventDefault()
            }

            window.addEventListener('mousemove', onmousemove, true)
            window.addEventListener('mouseup', onmouseup, true)

            evt.stopPropagation()
            evt.preventDefault()        		
        })
    }

    static get observedAttributes() {
        return ['orientation', 'secondinvisible']
    }

    attributeChangedCallback(attributeName, oldValue, newValue) {
        switch (attributeName) {
            case "orientation":
                if (newValue == "VERTICAL") 
                    this.splitterGrid.classList.add("vertical") 
                else 
                    this.splitterGrid.classList.remove("vertical") 
                break
            case "secondinvisible":
                if (newValue == "true") 
                    this.splitterGrid.classList.add("secondInvisible")
                else
                    this.splitterGrid.classList.remove("secondInvisible")
                break
        }
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

    render() {
        this.renderItems()
        this.renderScrollbarGrip()
    }
    
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

customElements.define('grid-splitter', GridSplitter)