interface RequestType {}
interface ResponseType {}

interface ChangePath extends RequestType  {
    path: string
}

interface ChangePathResponse extends ResponseType  {
    path?: string
}

export const changePath = getJsonPost<ChangePath, ChangePathResponse>("init")

function getJsonPost<RequestType, ResponseType>(method: string): (request: RequestType) => Promise<ResponseType> {
 
    async function jsonPost<RequestType, ResponseType>(method: string, request: RequestType): Promise<ResponseType> {
        const msg = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        }

        const response = await fetch(`http://localhost:20000/request/${method}`, msg)
        const json = await response.text()
        return JSON.parse(json) as ResponseType
    }

    return (request: RequestType) => jsonPost(method, request)
}