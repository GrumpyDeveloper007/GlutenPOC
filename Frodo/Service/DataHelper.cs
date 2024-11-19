using AutoMapper;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal static class DataHelper
    {
        public static List<PinTopic> ExtractPinExport(List<PinTopic> pins, List<Topic> topics, IMapper mapper)
        {
            foreach (var topic in topics)
            {
                var newT = mapper.Map<PinLinkInfo>(topic);
                if (topic.AiVenues != null)
                {
                    foreach (var aiVenue in topic.AiVenues)
                    {
                        var existingPin = DataHelper.IsPinInList(aiVenue.Pin, pins);
                        DataHelper.AddIfNotExists(pins, existingPin, newT, aiVenue.Pin);
                    }
                }

                if (topic.UrlsV2 != null)
                {
                    foreach (var url in topic.UrlsV2)
                    {
                        var existingPin = DataHelper.IsPinInList(url.Pin, pins);
                        DataHelper.AddIfNotExists(pins, existingPin, newT, url.Pin);
                    }
                }
            }
            return pins;
        }

        public static bool IsInList(List<AiVenue> venues, AiVenue venue, int newVenueIndex)
        {
            for (int i = 0; i < venues.Count; i++)
            {
                AiVenue? item = venues[i];
                if (i != newVenueIndex
                    && item.Pin != null && venue.Pin != null
                    && item.Pin.GeoLongitude == venue.Pin.GeoLongitude
                    && item.Pin.GeoLatatude == venue.Pin.GeoLatatude)
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddIfNotExists(List<PinTopic> pins, PinTopic? matchingPinTopic, PinLinkInfo topicToAdd, TopicPin? pinToAdd)
        {
            if (pinToAdd == null) return;
            if (pinToAdd == null) return;
            if (string.IsNullOrEmpty(pinToAdd.GeoLatatude)) return;
            if (string.IsNullOrEmpty(pinToAdd.GeoLongitude)) return;

            // if pin not found, add it to the list
            if (matchingPinTopic == null)
            {
                pins.Add(new PinTopic
                {
                    GeoLatatude = double.Parse(pinToAdd.GeoLatatude),
                    GeoLongitude = double.Parse(pinToAdd.GeoLongitude),
                    Label = pinToAdd.Label,
                    Topics = new List<PinLinkInfo> { topicToAdd }
                });
                return;
            }
            // dont add duplicates
            if (matchingPinTopic.Topics == null) matchingPinTopic.Topics = new List<PinLinkInfo>();
            foreach (var existingTopic in matchingPinTopic.Topics)
            {
                if (existingTopic.NodeID == topicToAdd.NodeID)
                {
                    return;
                }
            }
            matchingPinTopic.Topics.Add(topicToAdd);
        }

        public static PinTopic? IsPinInList(TopicPin? topicPin, List<PinTopic> pins)
        {
            foreach (var pin in pins)
            {
                if (topicPin != null &&
                    topicPin.GeoLatatude != null &&
                    topicPin.GeoLongitude != null &&
                    pin.GeoLatatude == double.Parse(topicPin.GeoLatatude) && pin.GeoLongitude == double.Parse(topicPin.GeoLongitude))
                {
                    return pin;
                }
            }
            return null;
        }


    }
}
