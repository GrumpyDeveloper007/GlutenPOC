'use client'

import React from 'react';
import { TopicGroupClass } from '../_model/model';

const Sidebar = ({ selectedTopicGroup }: { selectedTopicGroup: TopicGroupClass }) => {
    // Helper function to format a date
    const dateOnly = (date: Date): string => {
        return date.toDateString();
    };

    return (
        <div>
            {selectedTopicGroup ? (
                <div>
                    {/* Display topic group details */}
                    <h2>{selectedTopicGroup.label}</h2>
                    <p>{selectedTopicGroup.restaurantType}</p>
                    <p>{selectedTopicGroup.stars}</p>
                    <p>{selectedTopicGroup.price}</p>

                    {/* Conditional Google Maps link */}
                    {selectedTopicGroup.mapsLink && (
                        <a href={selectedTopicGroup.mapsLink} target="_blank" rel="noopener noreferrer">
                            Google Maps
                        </a>
                    )}

                    {/* Conditional highlighted description */}
                    {selectedTopicGroup.description && (
                        <p dangerouslySetInnerHTML={{
                            __html: highlightText(selectedTopicGroup.description, selectedTopicGroup.label),
                        }} />
                    )}

                    {/* Loop through topics */}
                    {selectedTopicGroup.topics.map((item, index) => (
                        <div key={index}>
                            {item.shortTitle && item.shortTitle.length > 0 ? (
                                <a href={item.facebookUrl} target="_blank" rel="noopener noreferrer" className="smallText">
                                    <div>{item.shortTitle}</div>
                                    <div>{dateOnly(item.postCreated)}</div>
                                </a>
                            ) : (
                                <a href={item.facebookUrl} target="_blank" rel="noopener noreferrer" className="smallText">
                                    Facebook
                                </a>
                            )}
                            <br />
                        </div>
                    ))}
                </div>
            ) : (
                <div>
                    {/* Default message when no topic group is selected */}
                    <h2>Welcome</h2>
                    <p>Please scroll the map and select a pin. Once selected a summary will be shown here.</p>
                    <p>There will also be links to Google Maps and Facebook groups.</p>
                    <p>Some groups are private so please join the group and share useful information for others.</p>
                    <p>
                        Remember this information may not be 100% accurate and up to date, always confirm the details with restaurant staff.
                    </p>
                    <a href="https://www.celiactravel.com/cards/">Gluten Free Restaurant Cards for Celiacs / Coeliacs</a>
                </div>
            )}
        </div>
    );
};

// Highlight function to simulate Angular's highlight pipe
const highlightText = (text: string, highlight: string): string => {
    if (!highlight) return text;
    const regex = new RegExp(`(${highlight})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
};

export default Sidebar;
