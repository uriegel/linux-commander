
// TODO Only in certain processors, not in root!

export const addDeleteJob = async (id, path, itemsToDelete) => {
    requests.deleteItems(id, path, itemsToDelete)
}