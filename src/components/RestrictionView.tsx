import { forwardRef, useImperativeHandle, useRef, useState } from 'react'
import type { FolderViewItem } from './FolderView'
import './RestrictionView.css'

export type RestrictionViewHandle = {
    checkKey: (code: string) => FolderViewItem[] | null
    reset: ()=>void
}

interface RestrictionViewProps {
    items: FolderViewItem[]
}

const RestrictionView = forwardRef<RestrictionViewHandle, RestrictionViewProps>((
    { items }, ref) => {

    const [restriction, setRestriction] = useState("")
    const refItems = useRef<FolderViewItem[]|null>(null)

    useImperativeHandle(ref, () => ({
        checkKey(code: string) {
            const test =
                code == "Backspace"
                ? restriction.length > 0 ? restriction.substring(0, restriction.length - 1) : ""
                : code == "Escape"
                ? ""
                : restriction + code

            if (test.length > 0) {
                const itemsToCheck = refItems.current ?? items
                const restrictedItems = itemsToCheck.filter(n => n.name.toLocaleLowerCase().startsWith(test))
                if (restrictedItems.length > 0) {
                    if (refItems.current == null)
                        refItems.current = items
                    setRestriction(test)
                    return restrictedItems
                }
                else
                    return null
            } else if (refItems.current != null) {
                const originalItems = refItems.current
                this.reset()
                return originalItems
            } else
                return null
        },
        reset() {
            setRestriction("")
            refItems.current = null
        }
    }))

    return (
        <input value={restriction} className={`restriction${restriction.length > 0 ? " show" : ""} `} readOnly />
    )
    
})



export default RestrictionView