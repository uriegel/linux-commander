
// TODO Only in certain processors, not in root!

export const addDeleteJob = async () => {
    console.log("Welche Seite und was?")

    const responseStr = await fetch("/commander/delete", { 
        method: 'POST', 
        headers: {
            'Content-Type': 'application/json'
        },            
        body: JSON.stringify({ 
            id: "id",
            path: "path",
            files: ["file1", "file2" ]
        }) 
    })
    console.log(`Welche Seite und was?, ${responseStr}`)
}