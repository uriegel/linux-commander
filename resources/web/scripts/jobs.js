
// TODO Only in certain processors, not in root!



const testsend = async (payload, id) => new Promise((res, rej) => {
    globres.set(id, res)
    sendMessageToWebView("test", payload)
})



export const addDeleteJob = async (id, path, itemsToDelete) => {


    //const ende = 100_000
    const ende = 10000

    const body = JSON.stringify({ 
        id,
        path,
        files: itemsToDelete
    }) 

    console.log("Warp", new Date())
    // for (let i = 0; i < ende; i++) {
    //     const responseStr = await fetch("/commander/delete", { 
    //         method: 'POST', 
    //         headers: {
    //             'Content-Type': 'application/json'
    //         },
    //         body            
    //     })
    //     const jason = await responseStr.json()
    //     //console.log("Warp ok", jason)
    // }

    console.log("Warp ok", new Date())


    let ok = await testsend(body, id)

    //console.log("Gtk ok", ok)

    for (let i = 0; i < ende; i++) {
        let ok = await testsend(body, id)
    }

    console.log("Gtk ok", new Date())

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