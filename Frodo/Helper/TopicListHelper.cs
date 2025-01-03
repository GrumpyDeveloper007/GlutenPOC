using Gluten.Core.Interface;
using Gluten.Core.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System.Web;

namespace Frodo.Helper
{
    /// <summary>
    /// General helpers for working with data structures
    /// </summary>
    internal static class TopicListHelper
    {
        public static IConsole Console { get; set; } = new DummyConsole();

        /// <summary>
        /// Removes existing topics associated with pins
        /// </summary>
        public static void CleanTopics(List<PinTopic> pins)
        {
            foreach (var item in pins)
            {
                item.Topics?.Clear();
            }
        }

        /// <summary>
        /// Removes empty pins
        /// </summary>
        public static void RemoveEmptyPins(List<PinTopic> pins)
        {
            for (int i = pins.Count - 1; i >= 0; i--)
            {
                var topics = pins[i].Topics;
                if (topics != null && topics.Count == 0)
                {
                    pins.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Clear the topic titles to make the data output smaller
        /// </summary>
        public static void RemoveTopicTitles(List<PinTopic> pins)
        {
            for (int i = pins.Count - 1; i >= 0; i--)
            {
                var topics = pins[i].Topics;
                if (topics != null)
                    foreach (var topic in topics)
                    {
                        topic.Title = "";
                    }
            }
        }

        /// <summary>
        /// is specified venue in the list (duplicated)?
        /// </summary>
        public static bool IsInList(List<AiVenue>? venues, AiVenue venue, int newVenueIndex)
        {
            if (venues == null) return false;
            for (int i = 0; i < venues.Count; i++)
            {
                AiVenue? item = venues[i];
                if (i != newVenueIndex
                    && item.IsChain == venue.IsChain
                    && item.ChainGenerated == venue.ChainGenerated
                    )
                {
                    // search done and pin found, details match
                    if (IsPinMatch(item.Pin, venue.Pin))
                    {
                        return true;
                    }

                    // search done and no pin found
                    if (item.PinSearchDone //&& venue.PinSearchDone
                        && item.Pin == null && venue.Pin == null
                        && item.PlaceName == venue.PlaceName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a topic to a pin, 
        /// or updates data if new (only really needed because of evolvoing data structures, TODO: can be cleaned up later)
        /// </summary>
        public static void AddIfNotExists(List<PinTopic> pins, PinTopic? matchingPinTopic,
                PinLinkInfo topicToAdd, TopicPin? pinToAdd, TopicPinCache? cachePin)
        {
            if (pinToAdd == null) return;
            if (pinToAdd == null) return;
            if (string.IsNullOrWhiteSpace(pinToAdd.GeoLatitude)) return;
            if (string.IsNullOrWhiteSpace(pinToAdd.GeoLongitude)) return;
            if (cachePin?.MetaData?.PermanentlyClosed == true) return;

            // if pin not found, add it to the list
            if (matchingPinTopic == null)
            {
                var newPin = new PinTopic
                {
                    GeoLatitude = double.Parse(pinToAdd.GeoLatitude),
                    GeoLongitude = double.Parse(pinToAdd.GeoLongitude),
                    Label = HttpUtility.UrlDecode(pinToAdd.Label ?? ""),
                    Topics = [topicToAdd],
                };
                if (cachePin != null && !string.IsNullOrWhiteSpace(cachePin.MapsUrl))
                {
                    newPin.MapsLink = cachePin.MapsUrl;
                    if (string.IsNullOrWhiteSpace(newPin.MapsLink))
                    {
                        Console.WriteLineRed($"Blank maps link :{newPin.Label}");
                    }

                    if (cachePin.MetaData != null)
                    {
                        newPin.RestaurantType = cachePin.MetaData.RestaurantType;
                        newPin.Stars = cachePin.MetaData.Stars;
                        newPin.Price = cachePin.MetaData.Price;
                    }
                }
                pins.Add(newPin);
                return;
            }
            matchingPinTopic.Topics ??= [];
            if (cachePin != null)
            {
                if (!string.IsNullOrWhiteSpace(cachePin.MapsUrl))
                {
                    matchingPinTopic.MapsLink = cachePin.MapsUrl;
                }
                if (string.IsNullOrWhiteSpace(matchingPinTopic.MapsLink))
                {
                    Console.WriteLineRed($"Blank maps link :{matchingPinTopic.Label}");
                }

                if (cachePin.MetaData != null)
                {
                    matchingPinTopic.Stars = cachePin.MetaData.Stars;
                    matchingPinTopic.RestaurantType = cachePin.MetaData.RestaurantType;
                    matchingPinTopic.Price = cachePin.MetaData.Price;
                }
                else
                {
                    System.Console.WriteLine("No meta data found");
                }
            }


            // Add topic to the pin - dont add duplicates
            foreach (var existingTopic in matchingPinTopic.Topics)
            {
                if (existingTopic.NodeID == topicToAdd.NodeID)
                {
                    return;
                }
            }
            matchingPinTopic.Topics.Add(topicToAdd);
        }

        /// <summary>
        /// Is this pin (geo location) already in our list
        /// </summary>
        public static PinTopic? IsPinInList(TopicPin? topicPin, List<PinTopic> pins)
        {
            foreach (var pin in pins)
            {
                if (topicPin != null &&
                    !string.IsNullOrWhiteSpace(topicPin.GeoLatitude) &&
                    !string.IsNullOrWhiteSpace(topicPin.GeoLongitude) &&
                    pin.GeoLatitude == double.Parse(topicPin.GeoLatitude) && pin.GeoLongitude == double.Parse(topicPin.GeoLongitude))
                {
                    return pin;
                }
            }
            return null;
        }

        /// <summary>
        /// Try to sync Topic data with data extracted from the web
        /// </summary>
        public static void CheckForUpdatedUrls(DetailedTopic topic, List<TopicLink> newUrls)
        {
            if (topic.UrlsV2 == null) return;
            if (topic.UrlsV2.Count != newUrls.Count)
            {
                Console.WriteLine("Mismatch in url detection");
            }
            else
            {
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    if (topic.UrlsV2[t].Pin == null)
                    {
                        if (topic.UrlsV2[t].Url != newUrls[t].Url
                            && !topic.UrlsV2[t].Url.Contains("/@")
                            && !topic.UrlsV2[t].Url.Contains("https://www.google.com/maps/d/viewer"))
                        {
                            topic.UrlsV2[t].Url = newUrls[t].Url;
                        }
                    }
                }
            }
        }

        private static bool IsPinMatch(TopicPin? original, TopicPin? newPin)
        {
            if (original != null && newPin != null
            && original.Label == newPin.Label
                && original.GeoLongitude == newPin.GeoLongitude
                && original.GeoLatitude == newPin.GeoLatitude
                )
            {
                return true;
            }
            return false;
        }

    }
}
