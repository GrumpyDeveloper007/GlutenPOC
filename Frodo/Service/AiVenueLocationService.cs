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
    public void UpdatePinsForAiVenues(List<DetailedTopic> Topics, bool regeneratePins)
    {
        var placeNameSkipList = _databaseLoaderService.LoadPlaceSkipList();

        var aiPinService = new AiVenueProcessorService(_mapPinService, _mappingService, _restaurantTypeService, Console);
        var pinsAdded = 0;

        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            // Remove any duplicated pins
            AiVenueDataHelper.RemoveDuplicatedVenues(topic.AiVenues);

            if (pinsAdded > 50)
            {
                _topicsDataLoaderService.SaveTopics(Topics);
                _databaseLoaderService.SavePlaceSkipList(placeNameSkipList);
                _databaseLoaderService.SavePinDB();
                pinsAdded = 0;
            }

            // Skip if the country and city cannot be identified
            var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
            if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(topic.TitleCity)) continue;

            var delayedConsoleLine = $"Update Pins For AI Venues {i} of {Topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}";
            if (i % 1000 == 0)
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];

                if (ai.IsChain) continue;
                if (!ai.IsExportable) continue;
                if (SearchCacheUpdatePin(ai, groupCountry))
                {
                    //Console.WriteLine($"Cached pin : {topic.AiVenues[t].PlaceName}");
                    continue;
                }
                if (ai.Pin != null && !regeneratePins) continue;

                // Only process unprocessed pins
                if (!ai.PinSearchDone || regeneratePins)
                {
                    if (!string.IsNullOrWhiteSpace(delayedConsoleLine))
                    {
                        Console.WriteLine(delayedConsoleLine);
                        delayedConsoleLine = null;
                    }

                    if (IsInSkipList(ai, placeNameSkipList))
                    {
                        Console.WriteLineBlue($"Skipping : {topic.AiVenues[t].PlaceName}");
                    }
                    else
                    {
                        if (ai.Pin == null || regeneratePins)
                        {
                            var chainUrls = new List<string>();
                            var city = topic.TitleCity;
                            if (string.IsNullOrWhiteSpace(city)) _fBGroupService.GetCityName(topic.GroupId);
                            aiPinService.GetMapPinForPlaceName(ai, groupCountry, topic.TitleCity, chainUrls, true);

                            // Add any chain urls detected earlier (searches that have multiple results)
                            foreach (var url in chainUrls)
                            {
                                var pin = PinHelper.GenerateMapPin(url, "", groupCountry);
                                if (pin != null)
                                {
                                    _mapPinCache.AddPinCache(pin, pin.Label);
                                    // Add pin to AiGenerated
                                    var newVenue = new AiVenue
                                    {
                                        PlaceName = pin.PlaceName,
                                        Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin),
                                        ChainGenerated = true
                                    };
                                    if (!DataHelper.IsInList(topic.AiVenues, newVenue, -1))
                                    {
                                        topic.AiVenues.Add(newVenue);
                                        Console.WriteLine($"Adding chain url {pin.Label}");
                                    }
                                }
                            }


                        }
                        if (ai.Pin == null)
                        {
                            // drop pin
                            if (!string.IsNullOrWhiteSpace(topic.AiVenues[t].PlaceName)
                                && !ai.IsChain)
                            {

                                if (!topic.AiVenues[t].PlaceName.Contains("Hotel"))
                                {
                                    Console.WriteLineRed($"Tagging pin as skippable : {topic.AiVenues[t].PlaceName}");
                                    placeNameSkipList.Add(topic.AiVenues[t]);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Found pin : {topic.AiVenues[t].PlaceName} : {pinsAdded}");
                            pinsAdded++;
                        }
                    }

                }

                ai.PinSearchDone = true;
            }
        }

        _topicsDataLoaderService.SaveTopics(Topics);
        _databaseLoaderService.SavePinDB();
        _databaseLoaderService.SavePlaceSkipList(placeNameSkipList);
    }

    public void UpdateChainGeneratedPins(List<DetailedTopic> Topics)
    {
        var placeNameSkipList = _databaseLoaderService.LoadPlaceSkipList();

        var aiPinService = new AiVenueProcessorService(_mapPinService, _mappingService, _restaurantTypeService, Console);
        var pinsAdded = 1;

        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            if (pinsAdded > 50)
            {
                _topicsDataLoaderService.SaveTopics(Topics);
                _databaseLoaderService.SavePlaceSkipList(placeNameSkipList);
                _databaseLoaderService.SavePinDB();
                pinsAdded = 0;
            }

            // Skip if the country and city cannot be identified
            var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
            if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(topic.TitleCity)) continue;

            if (i % 1000 == 0)
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];

                if (ai.IsChain) continue;
                if (ai.Pin == null) continue;
                if (SearchCacheUpdatePin(ai, groupCountry)) continue;

                // Only process unprocessed pins
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                var chainUrls = new List<string>();
                var city = topic.TitleCity;
                if (string.IsNullOrWhiteSpace(city)) _fBGroupService.GetCityName(topic.GroupId);
                if (!aiPinService.GetMapPinForPlaceName(ai, groupCountry, topic.TitleCity, chainUrls, false))
                    break;
            }
        }

        _topicsDataLoaderService.SaveTopics(Topics);
        _databaseLoaderService.SavePinDB();
        _databaseLoaderService.SavePlaceSkipList(placeNameSkipList);
    }


    public void CheckPinsAreInCache(List<DetailedTopic> Topics)
    {

        var aiPinService = new AiVenueProcessorService(_mapPinService, _mappingService, _restaurantTypeService, Console);
        var pinsAdded = 1;

        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            // Remove any duplicated pins
            AiVenueDataHelper.RemoveDuplicatedVenues(topic.AiVenues);

            if (pinsAdded > 20)
            {
                _topicsDataLoaderService.SaveTopics(Topics);
                _databaseLoaderService.SavePinDB();
                pinsAdded = 0;
            }

            // Skip if the country and city cannot be identified
            var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
            if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(topic.TitleCity)) continue;

            if (i % 1000 == 0)
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];

                if (ai.IsChain) continue;
                if (!ai.IsExportable) continue;
                if (SearchCacheUpdatePin(ai, groupCountry)) continue;
                if (ai.Pin == null) continue;

                // Only process unprocessed pins
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                var chainUrls = new List<string>();
                var city = topic.TitleCity;
                if (string.IsNullOrWhiteSpace(city)) _fBGroupService.GetCityName(topic.GroupId);
                if (!aiPinService.GetMapPinForPlaceName(ai, groupCountry, topic.TitleCity, chainUrls, false))
                    break;
            }
        }

        _topicsDataLoaderService.SaveTopics(Topics);
        _databaseLoaderService.SavePinDB();
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

    private bool IsInSkipList(AiVenue? ai, List<AiVenue> placeNameSkipList)
    {
        if (ai.PlaceName != null
    && ai.Address != null
    && placeNameSkipList.Exists(o =>
    ((o?.PlaceName ?? "").Contains(ai.PlaceName, StringComparison.InvariantCultureIgnoreCase)
    && o.Address != null && o.Address.Contains(ai.Address, StringComparison.InvariantCultureIgnoreCase))))
        {
            return true;
        }
        return false;
    }

    private bool SearchCacheUpdatePin(AiVenue? ai, string groupCountry)
    {
        if (ai == null) return false;

        TopicPinCache? cachedPin;
        if (ai.Pin != null)
        {
            cachedPin = _mapPinCache.TryGetPinLatLong(ai.Pin.GeoLatitude, ai.Pin.GeoLongitude);
            if (cachedPin != null)
            {
                ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
                return true;
            }
        }

        // alternate search
        cachedPin = _mapPinCache.TryGetPin(ai.PlaceName, groupCountry);
        if (cachedPin != null)
        {
            ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
            return true;
        }

        return false;
    }


}
