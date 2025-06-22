export interface History {
    set: (path: string) => void
    get: (reverse?: boolean) => string|null
}

export const initializeHistory = () => {
    const history: string[] = []
    let position = -1

    const set = (path: string) => {
        if (history[history.length -1] != path) {
            history.push(path)
            position = history.length - 1
        }
    }

    const getPrevious = () => 
        position > 0
        ? history[--position]
        : null    
    
    const getReverse = () => 
        position < history.length - 1
        ? history[++position]
        : null    
        
    const get = (reverse?: boolean) => 
        reverse
        ? getReverse()
        : getPrevious()

    return {
        set,
        get
    } as History
}