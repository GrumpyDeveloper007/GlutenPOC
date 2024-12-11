using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service;

/// <summary>
/// Handles the linking of AiVenue to map pins
/// </summary>
internal class AiVenueLocationService(MapPinService _mapPinService,
    DatabaseLoaderService _databaseLoaderService,
    MappingService _mappingService,
    GeoService _geoService,
    FBGroupService _fBGroupService,
    MapPinCache _mapPinCache,
    RestaurantTypeService _restaurantTypeService,
    TopicsDataLoaderService _topicsDataLoaderService,
    IConsole Console)
{

    /// <summary>
    /// Scan generated Ai Venues and try to generate any missing pins
    /// </summary>
    public void UpdatePinsForAiVenues(List<DetailedTopic> Topics, bool regeneratePins, List<AiVenue> _placeNameSkipList)
    {
        var aiPinService = new AiVenueProcessorService(_mapPinService, _mappingService, _restaurantTypeService, Console);

        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
            if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

            // Skip if the country and city cannot be identified
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(topic.TitleCity)) continue;

            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

            // Remove any duplicated pins
            AiVenueDataHelper.RemoveDuplicatedVenues(topic.AiVenues);

            var chainUrls = new List<string>();
            if (topic.AiVenues == null) continue;

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];
                // Only process unprocessed pins

                if (TryGetFromCache(ai, groupCountry)) continue;

                if (ai.Pin == null && !ai.PinSearchDone
                    || regeneratePins)
                {
                    if (ai.PlaceName != null
                        && ai.Address != null
                        && _placeNameSkipList.Exists(o =>
                        ((o.PlaceName ?? "").Contains(ai.PlaceName, StringComparison.InvariantCultureIgnoreCase)
                        && o.Address != null && o.Address.Contains(ai.Address, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        Console.WriteLine($"Skipping : {topic.AiVenues[t].PlaceName}");
                    }
                    else
                    {
                        if (ai.Pin == null || regeneratePins)
                        {
                            aiPinService.GetMapPinForPlaceName(ai, groupCountry, topic.TitleCity, chainUrls);
                        }
                        if (ai.Pin == null)
                        {
                            // drop pin
                            if (!string.IsNullOrWhiteSpace(topic.AiVenues[t].PlaceName)
                                && chainUrls.Count == 0)
                            {
                                _placeNameSkipList.Add(topic.AiVenues[t]);
                                Console.WriteLine($"Tagging pin as skippable : {topic.AiVenues[t].PlaceName}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Found pin : {topic.AiVenues[t].PlaceName}");

                        }
                    }

                }

                if (ai.Pin != null)
                {
                    if (!IsPinInGroupCountry(ai.Pin, topic))
                    {
                        Console.WriteLineRed($"Removing pin due to country : {topic.AiVenues[t].PlaceName}");
                        ai.Pin = null;
                    }
                }
                ai.PinSearchDone = true;
            }

            // Remove any pins that dont have names
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                if (string.IsNullOrWhiteSpace(topic.AiVenues[t].PlaceName))
                {
                    topic.AiVenues.RemoveAt(t);
                }
            }

            // Add any chain urls detected earlier (searches that have multiple results)
            foreach (var url in chainUrls)
            {
                var pin = PinHelper.GenerateMapPin(url, "", groupCountry);
                if (pin != null)
                {
                    // Add pin to AiGenerated
                    var newVenue = new AiVenue
                    {
                        Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin),
                    };
                    if (!DataHelper.IsInList(topic.AiVenues, newVenue, -1))
                    {
                        topic.AiVenues.Add(newVenue);
                        Console.WriteLine($"Adding chain url {pin.Label}");
                    }
                }
            }


        }
        _topicsDataLoaderService.SaveTopics(Topics);
        _databaseLoaderService.SavePinDB();
        _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
    }

    public bool IsPinInGroupCountry(TopicPin? pin, DetailedTopic topic)
    {
        if (pin != null)
        {
            var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
            if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

            if (string.IsNullOrWhiteSpace(groupCountry)) return true;
            var country = _geoService.GetCountryPin(pin);

            if (!string.IsNullOrWhiteSpace(country))
            {
                if (!groupCountry.Contains(country, StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = double.TryParse(pin.GeoLongitude, out double longitude);
                    _ = double.TryParse(pin.GeoLatitude, out double latitude);
                    Console.WriteLine($"country mismatch, group: {groupCountry} pin: {country}, {latitude}, {longitude}");
                    return false;
                }
            }
        }
        return true;
    }

    private bool TryGetFromCache(AiVenue? ai, string groupCountry)
    {
        if (ai == null) return false;
        var cachedPin = _mapPinCache.TryGetPin(ai.PlaceName, groupCountry);

        if (cachedPin != null)
        {
            // Filter pin types
            if (cachedPin.MetaData != null && _restaurantTypeService.IsRejectedRestaurantType(cachedPin.MetaData.RestaurantType))
            {
                Console.WriteLineRed("Found pin, but rejecting restaurant type");
                return false;
            }

            ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
            return true;
        }

        // alternate search
        if (ai.Pin != null)
        {
            cachedPin = _mapPinCache.TryGetPin(ai.Pin.Label, groupCountry);
            if (cachedPin != null)
            {

                // Filter pin types
                if (cachedPin.MetaData != null && _restaurantTypeService.IsRejectedRestaurantType(cachedPin.MetaData.RestaurantType))
                {
                    Console.WriteLineRed("Found pin, but rejecting restaurant type");
                    return false;
                }

                ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
                return true;
            }
        }
        return false;
    }


}
