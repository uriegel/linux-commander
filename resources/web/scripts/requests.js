export async function request(method, requestData) {
    const responseStr = await fetch(`/commander/${method}`, {
        method: 'POST', 
        headers: {
            'Content-Type': 'application/json'
        },            
        body: JSON.stringify(requestData || {})
    })
    return await responseStr.json()
}

