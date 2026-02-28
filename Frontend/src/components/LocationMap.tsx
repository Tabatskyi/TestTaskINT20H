import React from 'react';
import {MapContainer, TileLayer, Marker, useMapEvents} from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';

// Fix default marker icon paths broken by bundlers
L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

interface LocationMapProps {
    latitude: number;
    longitude: number;
    interactive?: boolean;
    onLocationChange?: (lat: number, lng: number) => void;
    height?: string;
}

const MapClickHandler: React.FC<{onLocationChange: (lat: number, lng: number) => void}> = ({onLocationChange}) => {
    useMapEvents({
        click(e) {
            onLocationChange(e.latlng.lat, e.latlng.lng);
        },
    });
    return null;
};

const LocationMap: React.FC<LocationMapProps> = ({
    latitude,
    longitude,
    interactive = false,
    onLocationChange,
    height = '300px',
}) => {
    return (
        <MapContainer
            center={[latitude, longitude]}
            zoom={10}
            style={{height, width: '100%', borderRadius: '8px', zIndex: 0}}
            scrollWheelZoom={true}
        >
            <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <Marker position={[latitude, longitude]}/>
            {interactive && onLocationChange && (
                <MapClickHandler onLocationChange={onLocationChange}/>
            )}
        </MapContainer>
    );
};

export default LocationMap;
