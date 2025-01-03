import React, { useState, useEffect, useRef, useCallback } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMapEvents } from 'react-leaflet';
import { restaurantTypes, Others } from '../_model/staticData';
import { apiService, locationService, mapDataService } from './_services/index';
import L from "leaflet";
import "leaflet/dist/leaflet.css";


const MapComponent = ({
    showHotels = true,
    showStores = true,
    showOthers = true,
    showGMPins = true,
}) => {
    const pinCache: { [key: string]: any } = {};
    const gmPinCache: { [key: string]: any } = {};

    const [restaurants, setRestaurants] = useState<{ Name: string; Show: boolean; }[]>([]);
    const [selectedTopicGroup, setSelectedTopicGroup] = useState(null);
    const [currentMarkers, setCurrentMarkers] = useState<any[]>([]);
    const [pinTopics, setPinTopics] = useState<any[]>([]);
    const [gmPins, setGmPins] = useState<any[]>([]);
    const [mapBounds, setMapBounds] = useState<L.LatLngBounds | null>(null);
    const mapRef = useRef<L.Map | null>(null);

    const loadMapPins = useCallback(() => {
        if (!mapBounds) return;

        const bounds = mapBounds;
        const countriesInView = mapDataService.getCountriesInView(bounds);
        const requests: Promise<any>[] = [];

        countriesInView.forEach((country) => {
            if (!pinCache[country]) {
                requests.push(
                    apiService.getPinTopic(country).then((data) => {
                        pinCache[country] = data;
                        setPinTopics((prev) => [...prev, ...data]);
                    })
                );
            }

            if (!gmPinCache[country]) {
                requests.push(
                    apiService.getGMPin(country).then((data) => {
                        gmPinCache[country] = data;
                        setGmPins((prev) => [...prev, ...data]);
                    })
                );
            }
        });

        Promise.all(requests).then(() => {
            showMapPins();
        });
    }, [mapBounds]);

    const showMapPins = useCallback(() => {
        const markers: any[] = [];
        pinTopics.forEach((pin) => {
            if (!mapBounds) return;
            if (pin.geoLatitude > mapBounds.getNorthEast().lat) return;
            if (pin.geoLatitude < mapBounds.getSouthWest().lat) return;
            if (pin.geoLongitude > mapBounds.getNorthEast().lng) return;
            if (pin.geoLongitude < mapBounds.getSouthWest().lng) return;
            console.log('showMapPins', pin.geoLatitude, pin.geoLongitude);

            if (
                (showHotels && pin.restaurantType === 'Hotel') ||
                (showStores && pin.restaurantType?.includes('store')) ||
                (showOthers && Others.includes(pin.restaurantType))
            ) {
                const marker = (
                    <Marker
                        key={pin.id}
                        position={[pin.geoLatitude, pin.geoLongitude]}
                        icon={L.icon({ iconUrl: getMarkerIcon(pin.restaurantType), iconSize: [36, 48] })}
                        eventHandlers={{
                            click: () => setSelectedTopicGroup(pin),
                        }}
                    >
                        <Popup>
                            <h3>{pin.label}</h3>
                        </Popup>
                    </Marker>
                );
                markers.push(marker);
            }
        });

        setCurrentMarkers(markers);
    }, [pinTopics, showHotels, showStores, showOthers, mapBounds]);

    const handleMapMove = () => {
        const map = mapRef.current;
        if (map) {
            setMapBounds(map.getBounds());
        }
    };

    useEffect(() => {
        const { location } = locationService();
        if (location.latitude && location.longitude) {
            apiService.postMapHome(location.latitude, location.longitude);
        }

        const initialRestaurants = restaurantTypes.map((type) => ({
            Name: type,
            Show: true,
        }));
        setRestaurants(initialRestaurants);
    }, []);

    useEffect(() => {
        loadMapPins();
    }, [loadMapPins]);

    return (
        <div>
            <MapContainer
                center={[35.6844, 139.753]}
                zoom={14}
                scrollWheelZoom={true}
                ref={mapRef}
                style={{ height: '500px', width: '100%' }}
            >
                <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
                {currentMarkers}
            </MapContainer>
            {selectedTopicGroup}
        </div>
    );
};

const getMarkerIcon = (restaurantType: string) => {
    // Logic to map restaurantType to specific icon URLs
    return 'default-icon.png'; // Replace with actual logic
};

export default MapComponent;


export class Restaurant {
    constructor(
        public Show: boolean,
        public Name: string,
    ) { }
}