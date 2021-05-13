export const DIRECTORY = "directory"

export const getDirectory = (folderId, path) => {
    const getType = () => DIRECTORY
    
    return {
        getType
    }
}