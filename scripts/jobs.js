
// TODO Only in certain processors, not in root!

export const addDeleteJob = async (id, path, itemsToDelete) => {
    console.log("Welche Seite und was?")

    const responseStr = await fetch("/commander/delete", { 
        method: 'POST', 
        headers: {
            'Content-Type': 'application/json'
        },            
        body: JSON.stringify({ 
            id,
            path,
            files: itemsToDelete
        }) 
    })
    console.log(`Welche Seite und was?, ${responseStr}`)
}