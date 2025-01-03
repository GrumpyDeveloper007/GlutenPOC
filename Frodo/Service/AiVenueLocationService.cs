﻿using Frodo.Helper;
using Gluten.Core.DataProcessing.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Frodo.Service;

/// <summary>
/// Handles the linking of AiVenue to map pins
/// </summary>
internal class AiVenueLocationService(
    DatabaseLoaderService _databaseLoaderService,
    MappingService _mappingService,
    GeoService _geoService,
    FBGroupService _fBGroupService,
    MapPinCacheService _mapPinCache,
    TopicsDataLoaderService _topicsDataLoaderService,
    AiVenueProcessorService _aiVenueProcessorService,
    MapPinService _mapPinService,
    IConsole console)
{
    private readonly IConsole Console = console;
    private readonly List<AiVenue> _placeNameSkipList = _databaseLoaderService.LoadPlaceSkipList();

    /// <summary>
    /// Scan generated Ai Venues and try to generate any missing pins
    /// </summary>
    public void ProcessNewPins(List<DetailedTopic> topics)
    {
        var pinsAdded = 0;

        for (int i = 0; i < topics.Count; i++)
        {
            //Manual - 4729
            //117916
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;
            if (TopicItemHelper.IsRecipe(topic)) continue;
            if (TopicItemHelper.IsTopicAQuestion(topic)) continue;

            // Skip if the country and city cannot be identified
            var groupCountry = GetCountry(topic);
            var city = GetCity(topic);
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(city)) continue;

            if (pinsAdded > 10)
            {
                _topicsDataLoaderService.SaveTopics(topics);
                _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
                _databaseLoaderService.SavePinDB();

                pinsAdded = 0;
            }

            var delayedConsoleLine = $"Processing New Ai Pins {i} of {topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}";
            if (i % 1000 == 0)
                Console.WriteLine($"Processing New Ai Pins {i} of {topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];

                if (ai.IsChain) continue;
                if (!ai.IsExportable) continue;
                if (ai.RejectedRestaurantType) continue;
                if (ai.Pin != null) continue;
                if (string.IsNullOrWhiteSpace(ai.PlaceName)) continue;
                //if (GetCachePin(ai, groupCountry, city) != null) continue;
                if (ai.PinSearchDone) continue;
                //if (IsInSkipList(ai, _placeNameSkipList)) continue;
                if (PlaceNameFilterHelper.StartsWithPlaceNameSkipList(ai.PlaceName)) continue;


                if (!string.IsNullOrWhiteSpace(delayedConsoleLine))
                {
                    Console.WriteLine(delayedConsoleLine);
                    delayedConsoleLine = null;
                }

                if (!GetMapPin(topic, ai, groupCountry, city, true))
                {
                    Console.WriteLine($"Failed to find pin : {ai.PlaceName} : {pinsAdded}");
                }
                if (ai.Pin == null && ai.ChainGenerated)
                {
                    //topic.AiVenues.RemoveAt(t);
                }
                if (ai.Pin != null)
                {

                    pinsAdded++;
                }

                ai.PinSearchDone = true;
            }
        }

        _topicsDataLoaderService.SaveTopics(topics);
        _databaseLoaderService.SavePinDB();
        _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
    }

    public void CheckPinsAreInCache(List<DetailedTopic> topics)
    {
        var pinsAdded = 1;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            if (pinsAdded > 20)
            {
                _topicsDataLoaderService.SaveTopics(topics);
                _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
                _databaseLoaderService.SavePinDB();
                pinsAdded = 0;
            }

            // Skip if the country and city cannot be identified
            var groupCountry = GetCountry(topic);
            var city = GetCity(topic);
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(city)) continue;

            if (i % 1000 == 0)
                Console.WriteLine($"Check Ai Pins are in cache {i} of {topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];

                if (ai.Pin == null) continue;
                if (ai.IsChain)
                {
                    ai.Pin = null;
                    continue;
                }
                if (GetCachePin(ai, groupCountry, city) != null) continue;
                if (string.IsNullOrWhiteSpace(ai.PlaceName)) continue;
                if (PlaceNameFilterHelper.StartsWithPlaceNameSkipList(ai.PlaceName)) continue;
                //if (IsInSkipList(ai, _placeNameSkipList)) continue;
                if (ai.ChainGenerated) continue;
                if (!ai.IsExportable) continue;

                Console.WriteLine($"Check Ai Pins are in cache {i} of {topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");
                if (!GetMapPin(topic, ai, groupCountry, city, !ai.ChainGenerated))
                {
                    ai.Pin = null;
                }
            }
        }

        _topicsDataLoaderService.SaveTopics(topics);
        _databaseLoaderService.SavePinDB();
    }

    public void CheckNonExportable(List<DetailedTopic> topics)
    {
        var pinsAdded = 1;
        var pinsUpdated = 0;

        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            if (pinsAdded > 20)
            {
                _topicsDataLoaderService.SaveTopics(topics);
                _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
                _databaseLoaderService.SavePinDB();
                pinsAdded = 0;
            }

            // Skip if the country and city cannot be identified
            var groupCountry = GetCountry(topic);
            var city = GetCity(topic);
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(topic.TitleCity)) continue;

            if (i % 1000 == 0)
                Console.WriteLine($"Check Non Exportable Ai Pins {i} of {topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];

                if (ai.IsChain) continue;
                if (ai.ChainGenerated) continue;
                if (ai.IsExportable) continue;
                if (ai.PermanentlyClosed) continue;
                if (ai.RejectedRestaurantType) continue;
                if (ai.Pin == null) continue;
                if (string.IsNullOrWhiteSpace(ai.PlaceName)) continue;
                if (PlaceNameFilterHelper.StartsWithPlaceNameSkipList(ai.PlaceName)) continue;
                //if (IsInSkipList(ai, _placeNameSkipList)) continue;

                var geoLat = ai.Pin.GeoLatitude;
                var geoLong = ai.Pin.GeoLongitude;
                var label = ai.Pin.Label;
                pinsAdded++;

                // Only process unprocessed pins
                Console.WriteLine($"Check Non Exportable Ai Pins {i} of {topics.Count} ({pinsUpdated}) : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                if (!GetMapPin(topic, ai, groupCountry, city, true))
                {
                    // remove existing pin match
                    ai.Pin = null;
                    break;
                }

                if (geoLat != ai.Pin.GeoLatitude || geoLong != ai.Pin.GeoLongitude)
                {
                    pinsUpdated++;
                    ai.IsExportable = true;
                    Console.WriteLineBlue($"Pin location updated, old label :{label} new:{ai.Pin.Label}");
                }
            }

        }

        Console.WriteLine($"Total pin location changes : {pinsUpdated} ");
        _topicsDataLoaderService.SaveTopics(topics);
        _databaseLoaderService.SavePinDB();
    }

    public void UpdateChainGeneratedPins(List<DetailedTopic> topics)
    {
        var pinsAdded = 1;

        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            if (pinsAdded > 50)
            {
                _topicsDataLoaderService.SaveTopics(topics);
                _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
                _databaseLoaderService.SavePinDB();
                pinsAdded = 0;
            }

            // Skip if the country and city cannot be identified
            var groupCountry = GetCountry(topic);
            var city = GetCity(topic);
            if (string.IsNullOrWhiteSpace(groupCountry) && string.IsNullOrWhiteSpace(city)) continue;

            if (i % 1000 == 0)
                Console.WriteLine($"Update Chain Generated Ai Pins {i} of {topics.Count} ");

            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                AiVenue? ai = topic.AiVenues[t];
                if (!ai.IsChain) continue;

                // Only process unprocessed pins
                Console.WriteLine($"Update Chain Generated Ai Pins {i} of {topics.Count} : {StringHelper.Truncate(topic.Title, 75).Replace("\n", "")}");

                GetMapPin(topic, ai, groupCountry, city, true);
            }
        }

        _topicsDataLoaderService.SaveTopics(topics);
        _databaseLoaderService.SavePinDB();
        _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
    }




    public void RemoveDuplicatedPins(List<DetailedTopic> topics)
    {
        int removeCount = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            // Remove any duplicated pins
            removeCount += AiVenueDataHelper.RemoveDuplicatedVenues(topic.AiVenues);
        }

        Console.WriteLine($"Duplicated pins removed {removeCount}");

        _topicsDataLoaderService.SaveTopics(topics);
    }


    private void AddChainUrls(List<string> chainUrls, string groupCountry, DetailedTopic topic)
    {
        // Add any chain urls detected earlier (searches that have multiple results)
        foreach (var url in chainUrls)
        {
            TopicPinCache? pin = null;
            var tempPin = PinHelper.GenerateMapPin(url, "", groupCountry);
            if (tempPin != null)
            {
                pin = _mapPinCache.TryGetPinLatLong(tempPin.GeoLatitude, tempPin.GeoLongitude, "");
            }
            if (pin == null)
            {
                _mapPinService.GoToUrl(url);
                pin = _mapPinService.TryToGenerateMapPin(url, "", groupCountry, "");
            }

            //var pin = PinHelper.GenerateMapPin(url, "", groupCountry);
            if (pin != null)
            {
                var newPin = _mapPinCache.AddPinCache(pin, pin.Label, pin.Label);
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
                if (!TopicListHelper.IsInList(topic.AiVenues, newVenue, -1))
                {
                    topic.AiVenues ??= [];
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
                if (!groupCountry.Contains(country, StringComparison.InvariantCultureIgnoreCase)
                    && !(groupCountry == "Peru" && country == "Peru(Peruvian point of view)"))
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

    private static bool IsInSkipList(AiVenue ai, List<AiVenue> placeNameSkipList)
    {
        if (ai.PlaceName != null
    && ai.Address != null
    && placeNameSkipList.Exists(o =>
    ((o?.PlaceName ?? "").Contains(ai.PlaceName, StringComparison.InvariantCultureIgnoreCase)
    && o?.Address != null && o.Address.Contains(ai.Address, StringComparison.InvariantCultureIgnoreCase))))
        {
            return true;
        }
        return false;
    }

    private TopicPinCache? GetCachePin(AiVenue? ai, string groupCountry, string city)
    {
        TopicPinCache? cachedPin = null;

        if (ai == null) return null;
        if (ai.Pin != null)
        {
            cachedPin = _mapPinCache.TryGetPinLatLong(ai.Pin.GeoLatitude, ai.Pin.GeoLongitude, ai.PlaceName);
            if (cachedPin != null)
            {
                return cachedPin;
            }

            // alternate search
            var placeName = _aiVenueProcessorService.FilterPlaceName(ai.PlaceName, groupCountry, city);
            if (ai.ChainGenerated && ai.Pin.Label != null) placeName = ai.Pin.Label;
            cachedPin = _mapPinCache.TryGetPinLatLong(ai.Pin.GeoLatitude, ai.Pin.GeoLongitude, placeName);
            if (cachedPin != null)
            {
                return cachedPin;
            }
        }

        return cachedPin;
    }



    private bool SearchCacheUpdatePin(AiVenue? ai, string groupCountry)
    {
        if (ai == null) return false;

        TopicPinCache? cachedPin;
        if (ai.Pin != null)
        {
            cachedPin = _mapPinCache.TryGetPinLatLong(ai.Pin.GeoLatitude, ai.Pin.GeoLongitude, ai.PlaceName);
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

    private string GetCountry(DetailedTopic topic)
    {
        var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
        if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";
        return groupCountry;
    }

    private string GetCity(DetailedTopic topic)
    {
        var city = topic.TitleCity;
        if (string.IsNullOrWhiteSpace(city)) city = _fBGroupService.GetCityName(topic.GroupId);
        return city;
    }


    private bool GetMapPin(DetailedTopic topic, AiVenue ai, string country, string? city, bool enableChainMatch)
    {
        var chainUrls = new List<string>();
        var result = _aiVenueProcessorService.GetMapPinForPlaceName(ai, country, city, chainUrls, true);

        // Add any chain urls detected earlier (searches that have multiple results)
        if (chainUrls.Count > 0)
        {
            if (enableChainMatch && !ai.ChainGenerated)
            {
                ai.Pin = null;
                ai.IsChain = true;
                AddChainUrls(chainUrls, country, topic);
            }
            else
            {
                // Look for exact match
                foreach (var item in chainUrls)
                {
                    _mapPinService.GoToUrl(item);
                    var pin = _mapPinService.TryToGenerateMapPin(item, "", "", "");

                    //var pin = PinHelper.GenerateMapPin(item, "", country);
                    if (pin != null && HttpUtility.UrlDecode(pin.Label) == ai.PlaceName)
                    {
                        if (pin.Label != ai.PlaceName) pin.SearchStrings.Add(ai.PlaceName);
                        var newPin = _mapPinCache.AddPinCache(pin, ai.PlaceName, pin.Label);
                        var topicPin = _mappingService.Map<TopicPin, TopicPinCache>(pin);
                        ai.Pin = topicPin;

                    }
                }
            }
        }
        if (!result && !string.IsNullOrWhiteSpace(ai.PlaceName) && !ai.IsChain
            && !PlaceNameFilterHelper.StartsWithPlaceNameSkipList(ai.PlaceName))
        {
            Console.WriteLineRed($"Tagging pin as skippable : {ai.PlaceName}");
            _placeNameSkipList.Add(ai);
        }

        ai.PinSearchDone = true;
        return result;

    }
}
