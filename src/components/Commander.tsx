import { forwardRef, useCallback, useEffect, useImperativeHandle, useRef, useState } from "react"
import ViewSplit from "view-split-react"
import type { FolderViewHandle, FolderViewItem } from "./FolderView"
import FolderView from "./FolderView"
import '../App.css'
import '../themes/adwaita.css'
import "functional-extensions"
import Statusbar from "./Statusbar"
import { cmdEvents, cmdToggleEvents, type CmdToggleMsg } from "../requests/events"
import PictureViewer from "./PictureViewer"

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

const Commander = forwardRef<CommanderHandle, object>((_, ref) => {

    useImperativeHandle(ref, () => ({
        onKeyDown
    }))

	const folderLeft = useRef<FolderViewHandle>(null)
	const folderRight = useRef<FolderViewHandle>(null)

	const [showViewer, setShowViewer] = useState(false)    
	const [showHidden, setShowHidden] = useState(false)
	const [itemProperty, setItemProperty] = useState<ItemProperty>({ path: "", latitude: undefined, longitude: undefined, isDirectory: false })
	const [itemCount, setItemCount] = useState({ dirCount: 0, fileCount: 0 })
	const [statusTextLeft, setStatusTextLeft] = useState<string | undefined>(undefined)
	const [statusTextRight, setStatusTextRight] = useState<string | undefined>(undefined)
	const [errorText, setErrorText] = useState<string | null>(null)
    
	const [activeFolderId, setActiveFolderId] = useState(ID_LEFT)
	const onFocusLeft = () => setActiveFolderId(ID_LEFT)
	const onFocusRight = () => setActiveFolderId(ID_RIGHT)

	const onKeyDown = (evt: React.KeyboardEvent) => {
		if (evt.code == "Tab" && !evt.shiftKey) {
			getInactiveFolder()?.setFocus()
			evt.preventDefault()
			evt.stopPropagation()
		}
	}

	const getActiveFolder = useCallback(() => activeFolderId == ID_LEFT ? folderLeft.current : folderRight.current, [activeFolderId])
	const getInactiveFolder = useCallback(() => activeFolderId == ID_LEFT ? folderRight.current : folderLeft.current, [activeFolderId])

	const onMenuAction = useCallback(async (key: string) => {
		switch (key) {
			case "refresh":
				getActiveFolder()?.refresh()
				break
			case "adaptpath": {
				const path = getActiveFolder()?.getPath()
				if (path)
					getInactiveFolder()?.changePath(path)
				break
			}
		}
	}, [getActiveFolder, getInactiveFolder])

	const onMenuToggleAction = useCallback(async (msg: CmdToggleMsg) => {
		switch (msg.cmd) {
			case "showhidden": 
				setShowHidden(msg.checked)
				folderLeft.current?.refresh(msg.checked)
				folderRight.current?.refresh(msg.checked)
				break
			case "showpreview":
				setShowViewer(msg.checked)
				break
		}
	}, [])

	useEffect(() => {
		folderLeft.current?.setFocus()
	}, [])

	useEffect(() => {
		const subscription = cmdEvents.subscribe(m => onMenuAction(m!.cmd))
		const subscriptionToggle = cmdToggleEvents.subscribe(onMenuToggleAction)
		return () => {
			subscriptionToggle.unsubscribe()
			subscription.unsubscribe()
		}
	}, [onMenuAction, onMenuToggleAction])

	const onItemChanged = useCallback(
		(path: string, isDirectory: boolean, latitude?: number, longitude?: number) => 
			setItemProperty({ path, isDirectory, latitude, longitude })
	, [])

	const onEnter = (item: FolderViewItem) => {
		getActiveFolder()?.processEnter(item)
	}

	const VerticalSplitView = () => (
		<ViewSplit firstView={FolderLeft} secondView={FolderRight}></ViewSplit>
    )
    
	const FolderLeft = () => (
		<FolderView ref={folderLeft} id={ID_LEFT} onFocus={onFocusLeft} onItemChanged={onItemChanged} onItemsChanged={setItemCount}
			onEnter={onEnter} showHidden={showHidden} setStatusText={setStatusTextLeft} />
	)
	const FolderRight = () => (
		<FolderView ref={folderRight} id={ID_RIGHT} onFocus={onFocusRight} onItemChanged={onItemChanged} onItemsChanged={setItemCount}
			onEnter={onEnter} showHidden={showHidden} setStatusText={setStatusTextRight} />
	)

	const getStatusText = useCallback(() => 
		activeFolderId == ID_LEFT ? statusTextLeft : statusTextRight
	, [activeFolderId, statusTextLeft, statusTextRight])

	const ViewerView = () => {
		const ext = itemProperty
					.path
					.getExtension()
					.toLocaleLowerCase()
		
		return ext == ".jpg" || ext == ".png" || ext == ".jpeg"
		// 	? previewMode == PreviewMode.Default
		 		? (<PictureViewer path={itemProperty.path} latitude={itemProperty.latitude} longitude={itemProperty.longitude} />)
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
		 	: (<div></div>)
	}

	return (
		<>
			<ViewSplit isHorizontal={true} firstView={VerticalSplitView} secondView={ViewerView} initialWidth={30} secondVisible={showViewer} />
			<Statusbar path={itemProperty.path} dirCount={itemCount.dirCount} fileCount={itemCount.fileCount}
					errorText={errorText} setErrorText={setErrorText} statusText={getStatusText()} />		
		</>
	)
})

    export default Commander