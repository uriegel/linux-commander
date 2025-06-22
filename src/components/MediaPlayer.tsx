import { getViewerPath } from '../controllers/controller'
import './MediaPlayer.css'

interface MediaPlayerProps {
    path: string
}

const MediaPlayer = ({ path }: MediaPlayerProps) => (
    <div className='viewer'>
        <video className='mediaPlayer' controls autoPlay
            src={getViewerPath(path)} />        
    </div>
)

export default MediaPlayer