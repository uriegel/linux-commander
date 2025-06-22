import { useRef } from 'react'
import './PictureViewer.css'
import { getViewerPath } from '../controllers/controller'

interface PictureViewerProps {
    path: string
    latitude?: number
    longitude?: number
}

const PictureViewer = ({ path, latitude, longitude }: PictureViewerProps) => {

    const nocache = useRef(0)

    console.log("PictureViewer", latitude, longitude)

    return (
        <div className='viewer'>
            <img className='viewerImg'
                src={`${getViewerPath(path)}?nochache=${nocache.current++}`} alt="kein Bild gefunden" />
        </div>
    )
}

export default PictureViewer

