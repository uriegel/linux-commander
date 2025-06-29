import { useEffect, useRef, useState } from "react"
import type { ExtensionProps } from "web-dialog-react"
import type { ExtendedRenameProps } from "../../controllers/directory"

const ExtendedRenamePart = ({props }: ExtensionProps) => {

    const extendedRenameProps = props as ExtendedRenameProps
    const [prefix, setPrefix] = useState(extendedRenameProps.prefix)
    const [digits, setDigits] = useState(`${extendedRenameProps.digits}`)
    const [startNumber, setStartNumber] = useState(`${extendedRenameProps.startNumber}`)

    const startNumberRef = useRef<HTMLInputElement>(null)

    const onPrefix = (val: string) => {
        setPrefix(val)
        extendedRenameProps.prefix = val
    }

    const onDigits = (val: string) => {
        setDigits(val)
        extendedRenameProps.digits = val.parseInt() ?? 1
    }

    const onStartNumber = (val: string) => {
        setStartNumber(val)
        extendedRenameProps.startNumber = val.parseInt() ?? 1
    }

    useEffect(() => {
        if (startNumberRef.current)
            startNumberRef.current.focus()
    }, [startNumberRef])
        
    const selectInput = (e: React.FocusEvent<HTMLInputElement>) => e.target?.select()

    return (
        <table>
            <tbody>
                <tr>
                    <td className="right">Prefix:</td>
                    <td>
                        <input className="wdr-focusable" type="text" value={prefix} onChange={e => onPrefix(e.target.value)} onFocus={selectInput} />
                    </td>
                </tr>
                <tr>
                    <td className="right">Stellen:</td>
                    <td>
                        <select value={digits} onChange={e => onDigits(e.target.value)} className="wdr-focusable">
                            <option>1</option>
                            <option>2</option>
                            <option>3</option>
                            <option>4</option>
                        </select>
                    </td>
                </tr>
                <tr>
                    <td className="right">Start:</td>
                    <td>
                        <input type="number" ref={startNumberRef} className="wdr-focusable" value={startNumber} onChange={e => onStartNumber(e.target.value)}
                        onFocus={selectInput} />
                    </td>
                </tr>
            </tbody>
        </table>
    )
}

export default ExtendedRenamePart