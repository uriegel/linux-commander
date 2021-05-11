import { getRoot } from "./root.js"


export const getProcessor = (folderId, path, recentProcessor) => {

    if (!path)
        path = localStorage.getItem(`${folderId}-path`) || "root"

    if (path == "root") {
        if (recentProcessor && recentProcessor.type == "root")
            return {
                processor: recentProcessor, 
                changed: false
            }
        else
            return {
                processor: getRoot(folderId), 
                changed: true
            }
    }
    else 
        return {
            processor: null, 
            changed: false
        }
}

  