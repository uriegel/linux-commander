export { }

declare global {
    interface String {
        appendPath(subPath: string): string 
        getExtension(): string
        extractSubPath(): string
        getParentPath(): string
    }
}

String.prototype.getExtension = function (): string {
    const index = this.lastIndexOf(".")
    return index > 0 ? this.substring(index) : ""
}

String.prototype.extractSubPath = function (): string {
    return this.substring(this.lastIndexOfAny(["/", "\\"]))
}

String.prototype.getParentPath = function (): string {
    return this.length > 1 && (this.charAt(this.length - 1) == "/" || this.charAt(this.length - 1) == "\\")
        ? this.substring(0, this.substring(0, this.length - 1).lastIndexOfAny(["/", "\\"]))
        : this.substring(0, this.lastIndexOfAny(["/", "\\"]))
}

String.prototype.appendPath = function (subPath: string): string {
    return this.endsWith("/") || subPath.startsWith('/')
        ? this + subPath
        : this + "/" + subPath
}

