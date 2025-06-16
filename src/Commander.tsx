import { forwardRef, useImperativeHandle, useRef, useState } from "react"
import ViewSplit from "view-split-react"
import type { FolderViewHandle } from "./components/FolderView"
import FolderView from "./components/FolderView"
import './App.css'
import './themes/adwaita.css'

const ID_LEFT = "left"
const ID_RIGHT = "right"


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

	const folderLeft = useRef<FolderViewHandle>(null)
	const folderRight = useRef<FolderViewHandle>(null)

    const [showViewer, setShowViewer] = useState(false)    
    
	const FolderLeft = () => (
		<FolderView ref={folderLeft} id={ID_LEFT} />
	)
	const FolderRight = () => (
		<FolderView ref={folderRight} id={ID_RIGHT} />
	)


	const VerticalSplitView = () => (
		<ViewSplit firstView={FolderLeft} secondView={FolderRight}></ViewSplit>
    )
    
    const ViewerView = () => {
        return <div>Der Viewer</div>
		// const ext = path
		// 			.path
		// 			.getExtension()
		// 			.toLocaleLowerCase()
		
		// return ext == ".jpg" || ext == ".png"
		// 	? previewMode == PreviewMode.Default
		// 		? (<PictureViewer path={path.path} latitude={path.latitude} longitude={path.longitude} />)
		// 		: previewMode == PreviewMode.Location && path.latitude && path.longitude
		// 		? (<LocationViewer latitude={path.latitude} longitude={path.longitude} />)
		// 		: path.latitude && path.longitude
		// 		? <div className='bothViewer'>
		// 				<PictureViewer path={path.path} latitude={path.latitude} longitude={path.longitude} />
		// 				<LocationViewer latitude={path.latitude} longitude={path.longitude} />
		// 			</div>	
		// 		:(<PictureViewer path={path.path} latitude={path.latitude} longitude={path.longitude} />)
		// 	: ext == ".mp3" || ext == ".mp4" || ext == ".mkv" || ext == ".wav"
		// 	? (<MediaPlayer path={path.path} />)
		// 	: ext == ".pdf"
		// 	? (<FileViewer path={path.path} />)
		// 	: ext == ".gpx"
		// 	? (<TrackViewer path={path.path} />)
		// 	: (<div></div>)
	}

    return (
        <ViewSplit isHorizontal={true} firstView={VerticalSplitView} secondView={ViewerView} initialWidth={30} secondVisible={showViewer} />    )
})

    export default Commander