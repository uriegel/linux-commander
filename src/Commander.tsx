import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react"
import ViewSplit from "view-split-react"
import type { FolderViewHandle } from "./components/FolderView"
import FolderView from "./components/FolderView"
import './App.css'
import './themes/adwaita.css'
import Statusbar from "./components/Statusbar"

const ID_LEFT = "left"
const ID_RIGHT = "right"

interface ItemProperty {
	path: string
	latitude?: number 
	longitude?: number
	isDirectory: boolean
}


export type CommanderHandle = {
    onKeyDown: (evt: React.KeyboardEvent)=>void
}

type CommanderProps = {
//    isMaximized: boolean
}


const Commander = forwardRef<CommanderHandle, CommanderProps>(({}, ref) => {

    useImperativeHandle(ref, () => ({
        onKeyDown
    }))

	const folderLeft = useRef<FolderViewHandle>(null)
	const folderRight = useRef<FolderViewHandle>(null)

	const [showViewer, setShowViewer] = useState(false)    
	const [itemProperty, setItemProperty] = useState<ItemProperty>({ path: "", latitude: undefined, longitude: undefined, isDirectory: false })
	const [itemCount, setItemCount] = useState({ dirCount: 0, fileCount: 0 })
	const [statusText, setStatusText] = useState<string | null>(null)
	const [errorText, setErrorText] = useState<string | null>(null)
    
	const FolderLeft = () => (
		<FolderView ref={folderLeft} id={ID_LEFT} onFocus={onFocusLeft} onItemsChanged={setItemCount} />
	)
	const FolderRight = () => (
		<FolderView ref={folderRight} id={ID_RIGHT} onFocus={onFocusRight} onItemsChanged={setItemCount} />
	)

	const activeFolderId = useRef("left")
//	const getActiveFolder = () => activeFolderId.current == ID_LEFT ? folderLeft.current : folderRight.current
	const getInactiveFolder = () => activeFolderId.current == ID_LEFT ? folderRight.current : folderLeft.current

	const onFocusLeft = () => activeFolderId.current = ID_LEFT
	const onFocusRight = () => activeFolderId.current = ID_RIGHT

	const onKeyDown = (evt: React.KeyboardEvent) => {
		if (evt.code == "Tab" && !evt.shiftKey) {
			getInactiveFolder()?.setFocus()
			evt.preventDefault()
			evt.stopPropagation()
		}
	}

	useEffect(() => {
		folderLeft.current?.setFocus()
	}, [])


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
		<>
			<ViewSplit isHorizontal={true} firstView={VerticalSplitView} secondView={ViewerView} initialWidth={30} secondVisible={showViewer} />
			<Statusbar path={itemProperty.path} dirCount={itemCount.dirCount} fileCount={itemCount.fileCount}
					errorText={errorText} setErrorText={setErrorText} statusText={statusText} />		
		</>
	)
})

    export default Commander