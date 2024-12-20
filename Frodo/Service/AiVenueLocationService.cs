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
                            if (string.IsNullOrWhiteSpace(city)) city = _fBGroupService.GetCityName(topic.GroupId);
                            aiPinService.GetMapPinForPlaceName(ai, groupCountry, city, chainUrls, true);

                            // Add any chain urls detected earlier (searches that have multiple results)
                            ai.Pin = null;
                            ai.IsChain = true;
                            AddChainUrls(chainUrls, groupCountry, topic);
                        }
                        if (ai.Pin == null)
                        {
                            // drop pin
                            if (!string.IsNullOrWhiteSpace(ai.PlaceName)
                                && !ai.IsChain)
                            {

                                if (!ai.PlaceName.Contains("Hotel"))
                                {
                                    Console.WriteLineRed($"Tagging pin as skippable : {topic.AiVenues[t].PlaceName}");
                                    placeNameSkipList.Add(ai);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Found pin : {ai.PlaceName} : {pinsAdded}");
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

                if (!ai.IsChain) continue;
                //if (SearchCacheUpdatePin(ai, groupCountry)) continue;

                // Only process unprocessed pins
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                var chainUrls = new List<string>();
                var city = topic.TitleCity;
                if (string.IsNullOrWhiteSpace(city)) city = _fBGroupService.GetCityName(topic.GroupId);
                aiPinService.GetMapPinForPlaceName(ai, groupCountry, city, chainUrls, true);

                if (chainUrls.Count == 0 && ai.IsChain)
                {
                    Console.WriteLineBlue($"No chains found");
                }
                // Add any chain urls detected earlier (searches that have multiple results)
                ai.Pin = null;
                ai.IsChain = true;
                AddChainUrls(chainUrls, groupCountry, topic);
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

                if (SearchCacheUpdatePin(ai, groupCountry)) continue;
                if (ai.IsChain) continue;
                if (!ai.IsExportable) continue;
                if (ai.Pin == null) continue;

                // Only process unprocessed pins
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                var chainUrls = new List<string>();
                var city = topic.TitleCity;
                if (string.IsNullOrWhiteSpace(city)) city = _fBGroupService.GetCityName(topic.GroupId);
                if (!aiPinService.GetMapPinForPlaceName(ai, groupCountry, city, chainUrls, false))
                    break;
            }
        }

        _topicsDataLoaderService.SaveTopics(Topics);
        _databaseLoaderService.SavePinDB();
    }


    public void RemoveDuplicatedPins(List<DetailedTopic> Topics)
    {
        int removeCount = 0;
        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            // Remove any duplicated pins
            removeCount += AiVenueDataHelper.RemoveDuplicatedVenues(topic.AiVenues);
        }

        Console.WriteLine($"Duplicated pins removed {removeCount}");

        _topicsDataLoaderService.SaveTopics(Topics);
    }

    public void CheckNonExportable(List<DetailedTopic> Topics)
    {
        var aiPinService = new AiVenueProcessorService(_mapPinService, _mappingService, _restaurantTypeService, Console);
        var pinsAdded = 1;
        var pinsUpdated = 0;

        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;
            if (i < 32134) continue;

            //17302
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
                if (ai.ChainGenerated) continue;
                if (ai.IsExportable) continue;
                if (ai.PermanentlyClosed) continue;
                if (ai.RejectedRestaurantType) continue;
                if (ai.Pin == null) continue;

                var geoLat = ai.Pin.GeoLatitude;
                var geoLong = ai.Pin.GeoLongitude;
                var label = ai.Pin.Label;
                var cachedPin = _mapPinCache.TryGetPinLatLong(ai.Pin.GeoLatitude, ai.Pin.GeoLongitude);
                pinsAdded++;

                // Only process unprocessed pins
                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} ({pinsUpdated}) : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                var chainUrls = new List<string>();
                var city = topic.TitleCity;
                if (string.IsNullOrWhiteSpace(city)) city = _fBGroupService.GetCityName(topic.GroupId);
                if (!aiPinService.GetMapPinForPlaceName(ai, groupCountry, city, chainUrls, true))
                {
                    // remove existing pin match
                    ai.Pin = null;
                    break;
                }

                if (chainUrls.Count > 0)
                {
                    Console.WriteLine($"Setting pin as chain");
                    ai.PinSearchDone = false;
                    ai.Pin = null;
                    ai.IsChain = true;
                    ai.IsExportable = true;

                    AddChainUrls(chainUrls, groupCountry, topic);
                    continue;
                }


                if (geoLat != ai.Pin.GeoLatitude || geoLong != ai.Pin.GeoLongitude)
                {
                    pinsUpdated++;
                    ai.IsExportable = true;
                    Console.WriteLineBlue($"Pin location updated, old label :{label} new:{ai.Pin.Label}");
                }
                //if (!_aiVenueLocationService.IsPinInGroupCountry(pin, topic))
                //{
                //    var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                //    if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                //    var country = _geoService.GetCountryPin(cachePin);
                //}
            }

        }

        Console.WriteLine($"Total pin location changes : {pinsUpdated} ");
        _topicsDataLoaderService.SaveTopics(Topics);
        _databaseLoaderService.SavePinDB();
    }

    private void AddChainUrls(List<string> chainUrls, string groupCountry, DetailedTopic topic)
    {
        // Add any chain urls detected earlier (searches that have multiple results)
        foreach (var url in chainUrls)
        {
            var pin = PinHelper.GenerateMapPin(url, "", groupCountry);
            if (pin != null)
            {
                var newPin = _mapPinCache.AddPinCache(pin, pin.Label);
                if (newPin.PlaceName.StartsWith("https://"))
                {
                    Console.WriteLine($"Fixing place name");
                    newPin.PlaceName = pin.PlaceName;
                }
                if (newPin != null) pin = newPin;
                // Add pin to AiGenerated
                var newVenue = new AiVenue
                {
                    PlaceName = pin.PlaceName,
                    Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin),
                    ChainGenerated = true
                };
                if (!DataHelper.IsInList(topic.AiVenues, newVenue, -1, true))
                {
                    topic.AiVenues.Add(newVenue);
                    Console.WriteLine($"Adding chain url :{newVenue.PlaceName}");
                }
            }
        }

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
                    //Console.WriteLine($"country mismatch, group: {groupCountry} pin: {country}, {latitude}, {longitude}");
                    return false;
                }
            }
        }
        return true;
    }

    private static bool IsInSkipList(AiVenue ai, List<AiVenue> placeNameSkipList)
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
                ai.PermanentlyClosed = cachedPin?.MetaData?.PermanentlyClosed ?? false;
                return true;
            }
        }

        // alternate search
        cachedPin = _mapPinCache.TryGetPin(ai.PlaceName, groupCountry);
        if (cachedPin != null)
        {
            ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
            ai.PermanentlyClosed = cachedPin?.MetaData?.PermanentlyClosed ?? false;
            return true;
        }

        return false;
    }


}
