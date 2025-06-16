type RequestType = {}
type ResponseType = {}

export async function jsonPost(method: string, request: RequestType): Promise<ResponseType> {
 
    const msg = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
    }

    const response = await fetch(`http://localhost:20000/request/${method}`, msg)
    const json = await response.text()
    return JSON.parse(json) as ResponseType
}