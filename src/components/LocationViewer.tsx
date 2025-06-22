import 'leaflet/dist/leaflet.css'
import './LocationViewer.css'
import useResizeObserver from '@react-hook/resize-observer'
import { Map as LMap } from "leaflet"
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import { useEffect, useRef } from 'react'

type LocationViewerProps = {
    latitude: number
    longitude: number
}

const LocationViewer = ({ latitude, longitude }: LocationViewerProps) => {

    const myMap = useRef<LMap | null>(null)
    const root = useRef<HTMLDivElement>(null)

    useEffect(() => {
        myMap.current?.setView([latitude!, longitude!])
    }, [latitude, longitude])

    useResizeObserver(root.current, () => {
        myMap.current?.invalidateSize({ debounceMoveend: true, animate: true })    
    })

    return (
        <div className="locationView" ref={root}>
            <MapContainer className='location' ref={myMap} center={[latitude, longitude]} zoom={13} scrollWheelZoom={true} >
                <TileLayer attribution='' url="https://{s}.tile.openstreetmap.de/tiles/osmde/{z}/{x}/{y}.png" />
                <Marker position={[latitude, longitude]}>
                    <Popup>Der Aufnahmestandort.</Popup>
                </Marker>
            </MapContainer>
        </div>
    )
}

export default LocationViewer


