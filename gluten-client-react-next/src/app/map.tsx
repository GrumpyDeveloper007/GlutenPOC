'use client'

import React, { useState, useCallback, useEffect, useMemo } from "react";
import { MapContainer, Marker, Popup, TileLayer } from "react-leaflet";
import CustomMarker from "./custom-marker";
import { LatLngTuple } from "leaflet";
import { Map as LeafletMap } from 'leaflet';
import { apiService, locationService, mapDataService } from './_services/index';
import L from "leaflet";

const center: LatLngTuple = [51.505, -0.09]
const zoom = 13

interface DisplayPositionProps {
    map: LeafletMap;
    setCurrentMarkers: React.Dispatch<React.SetStateAction<any[]>>;
}

function DisplayPosition({ map, setCurrentMarkers }: DisplayPositionProps) {
    const [position, setPosition] = useState(() => map.getCenter())
    const [pinCount, setPinCount] = useState(0)
    const [countriesInView, setcountriesInView] = useState<string[]>([]);
    const [pinTopics, setPinTopics] = useState<any[]>([]);
    const [gmPins, setGmPins] = useState<any[]>([]);

    const [pinCache, setpinCache] = useState<{ [key: string]: any }>({});
    const [gmPinCache, setgmPinCache] = useState<{ [key: string]: any }>({});


    const onClick = useCallback(() => {
        map.setView(center, zoom)
    }, [map])

    const onMove = useCallback(() => {
        var countriesInView = mapDataService.getCountriesInView(map.getBounds());
        setcountriesInView(countriesInView);
        console.log(countriesInView);
        const requests: Promise<any>[] = [];


        countriesInView.forEach((country) => {
            console.log('country', country);
            console.log('pinCache :' + pinCache[country]);
            if (!(country in pinCache)) {
                requests.push(
                    apiService.getPinTopic(country).then((data) => {
                        setpinCache((prev) => { prev[country] = data; return prev; });
                        setPinTopics((prev) => [...prev, ...data]);
                    })
                );
            }

            /*             if (!gmPinCache[country]) {
                            requests.push(
                                apiService.getGMPin(country).then((data) => {
                                    gmPinCache[country] = data;
                                    setGmPins((prev) => [...prev, ...data]);
                                })
                            );
                        } */
        });
        Promise.all(requests).then(() => {
            showMapPins();
        });

        setPosition(map.getCenter())
    }, [map, pinTopics, pinCache])

    const showMapPins = useCallback(() => {
        var pinCount = 0;
        const markers: any[] = [];
        const customIcon = L.icon({
            iconUrl: '/BBQ.png',
            iconSize: [50, 50],
            iconAnchor: [25, 50],
        });
        console.log('showMapPins count ', pinTopics.length);

        pinTopics.forEach((pin) => {
            const mapBounds = map.getBounds();
            if (pin.geoLatitude > mapBounds.getNorthEast().lat) return;
            if (pin.geoLatitude < mapBounds.getSouthWest().lat) return;
            if (pin.geoLongitude > mapBounds.getNorthEast().lng) return;
            if (pin.geoLongitude < mapBounds.getSouthWest().lng) return;
            console.log('showMapPins', pin.geoLatitude, pin.geoLongitude);
            pinCount++;

            const marker = (
                <Marker
                    key={pin.id}
                    position={[pin.geoLatitude, pin.geoLongitude]}
                    icon={customIcon}
                >
                    <Popup>
                        <h3>{pin.label}</h3>
                    </Popup>
                </Marker>
            );
            markers.push(marker);
        });
        setPinCount(pinCount);

        setCurrentMarkers(markers);
    }, [pinTopics, setCurrentMarkers]);

    useEffect(() => {
        showMapPins();
    }, [pinTopics, showMapPins]);

    useEffect(() => {
        map.on('moveend', onMove)
        return () => {
            map.off('moveend', onMove)
        }
    }, [map, onMove])

    return (
        <p>
            pins:{pinCount}, latitude: {position.lat.toFixed(4)}, longitude: {position.lng.toFixed(4)}{' '},
            countriesInView: {countriesInView.map((country: any) => country).join(', ')},
            pinCount: {pinTopics.length}
            <button onClick={onClick}>reset</button>
        </p>
    )
}


export default function Map() {
    const [map, setMap] = useState<LeafletMap | null>(null);
    const [currentMarkers, setCurrentMarkers] = useState<any[]>([]);

    const displayMap = useMemo(
        () => (
            <MapContainer className="map-wrap" style={{ height: "400px", width: "800px" }}
                center={center}
                zoom={zoom}
                scrollWheelZoom={true}
                ref={setMap}>
                <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {currentMarkers}
                <CustomMarker position={[51.505, -0.09]}>
                    <Popup>This is a custom marker popup</Popup>
                </CustomMarker>
            </MapContainer>
        ),
        [currentMarkers],
    )

    return (
        <div className="map-wrap">
            {map ? <DisplayPosition map={map} setCurrentMarkers={setCurrentMarkers} /> : null}
            {displayMap}
        </div>
    )
}