export class ImageViewer extends HTMLElement {
    constructor() {
        super()

        const shadowRoot = this.attachShadow({ mode: "open" })
        const link = document.createElement("link")
        link.rel = "stylesheet"
        link.href = "../styles/imageviewer.css"
        shadowRoot.appendChild(link);

        let template = document.getElementById("image-viewer") as HTMLTemplateElement
        let templateContent = template.content
        shadowRoot.appendChild(templateContent.cloneNode(true))
        this.image = shadowRoot.getElementById('viewer') as HTMLImageElement
    }

    set path(val: string) {
        this.image.src = val
    }

    image: HTMLImageElement
}

export function test(str: string) {
    console.log("das ist vom test", str)
}

customElements.define('image-viewer', ImageViewer)