import { forwardRef, useImperativeHandle } from "react"

export type CommanderHandle = {
//    onKeyDown: (evt: React.KeyboardEvent)=>void
}

type CommanderProps = {
//    isMaximized: boolean
}


const Commander = forwardRef<CommanderHandle, CommanderProps>(({}, ref) => {

    useImperativeHandle(ref, () => ({
        //onKeyDown
    }))

  	return (
		<>
        </>
    )
})

    export default Commander