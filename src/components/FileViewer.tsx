import { getViewerPath } from '../controllers/controller'
import './FileViewer.css'

interface FileViewerProps {
    path: string
}

const FileViewer = ({ path }: FileViewerProps) => (
    <div className='viewer'>
        <iframe title="file viewer" className="fileViewer"
            src={getViewerPath(path)} />
    </div>
)

export default FileViewer