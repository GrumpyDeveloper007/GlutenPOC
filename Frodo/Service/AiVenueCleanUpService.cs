using Frodo.Helper;
using Gluten.Core.DataProcessing.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service;

internal class AiVenueCleanUpService(
    GeoService _geoService,
    FBGroupService _fBGroupService,
    MapPinCache _mapPinCache,
    AiVenueLocationService _aiVenueLocationService,
    TopicsDataLoaderService _topicsService,
    IConsole Console)
{
    public void TagAiPinsInFoundInDifferentCountry(List<DetailedTopic> topics)
    {
        LabelHelper.Reset();
        var invalidGeo = 0;
        var unmatchedLabels = 0;
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (i % 1000 == 0)
                Console.WriteLine($"Looking for invalid pins due to country {i} of {topics.Count}");

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                var pin = venue.Pin;

                if (pin == null) continue;

                var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude);
                if (cachePin == null) continue;

                if (!_aiVenueLocationService.IsPinInGroupCountry(pin, topic))
                {
                    var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                    if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                    var country = _geoService.GetCountryPin(cachePin);
                    invalidGeo++;
                    Console.WriteLine($"Tagging pin {t} in topic {i} - country mismatch {groupCountry}, {country}");
                    venue.IsExportable = false;
                    continue;
                }
                if (cachePin.MetaData != null && cachePin.MetaData.PermanentlyClosed)
                {
                    Console.WriteLine($"Tagging pin {t} in topic {i} - PermanentlyClosed");
                    venue.IsExportable = false;
                }
            }
        }

        LabelHelper.Check();
        Console.WriteLine($"Unmatched labels {unmatchedLabels}");
        Console.WriteLine($"Invalid pins {invalidGeo}");
        _topicsService.SaveTopics(topics);
    }

    public void TagAiPinsInNotFoundInOriginalText(List<DetailedTopic> topics)
    {
        LabelHelper.Reset();
        var invalidGeo = 0;
        var unmatchedLabels = 0;
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (i % 1000 == 0)
                Console.WriteLine($"Looking for pins not found in topic text {i} of {topics.Count}");

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                var pin = venue.Pin;

                // Try to filter out invalid pins
                if (!LabelHelper.IsInTextBlock(venue.PlaceName, topic.Title) && !venue.ChainGenerated)
                {
                    if (pin != null)
                    {
                        if (!LabelHelper.IsInTextBlock(pin.Label, topic.Title))
                        {
                            //Console.WriteLine($"Removing pin {t} in topic {i}");
                            Console.WriteLine($"missing label in title, label :{venue.PlaceName} pin:{pin.Label}  :{topic.Title}");
                            venue.IsExportable = false;
                            unmatchedLabels++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Tagging pin {t} in topic {i}");
                        venue.IsExportable = false;
                        unmatchedLabels++;
                    }
                }

            }

        }

        LabelHelper.Check();
        Console.WriteLine($"Unmatched labels {unmatchedLabels}");
        Console.WriteLine($"Invalid pins {invalidGeo}");
        _topicsService.SaveTopics(topics);
    }

    public void DiscoverMinMessageLength(List<DetailedTopic> topics)
    {
        var minLengthNoVenue = 9999;
        var topicId = 0;
        var noDataTopics = new List<string>();
        LabelHelper.Reset();
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null)
            {
                if (topic.Title.Contains("?")) noDataTopics.Add(topic.Title);
                continue;
            }
            if (i % 10000 == 0)
                Console.WriteLine($"Data collection {i} of {topics.Count}");

            if (minLengthNoVenue > topic.Title.Length)
            {
                topicId = i;
                minLengthNoVenue = topic.Title.Length;
            }

        }
        Console.WriteLine($"Min message length with AI discovery {minLengthNoVenue} : {topics[topicId].Title} ");
        //_topicsService.SaveTopics(Topics);
    }


    public void RemoveGenericPlaceNames(List<DetailedTopic> topics)
    {
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                if (PlaceNameFilterHelper.IsInPlaceNameSkipList(venue.PlaceName))
                {
                    if (venue.Pin != null) Console.WriteLineBlue($"Generic place with pin {venue.PlaceName}");
                    Console.WriteLine($"Removing {venue.PlaceName}");
                    topic.AiVenues.RemoveAt(t);
                }
            }
        }
        _topicsService.SaveTopics(topics);
    }

    public void RemoveNullAiPins(List<DetailedTopic> topics)
    {
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                if (topic.AiVenues[t] == null) topic.AiVenues.RemoveAt(t);
            }
        }
        _topicsService.SaveTopics(topics);
    }

    public void RemoveChainGeneratedAiPins(List<DetailedTopic> topics)
    {
        Console.WriteLine("--------------------------------------");
        int actionCount = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                if (venue.Pin == null) continue;

                if (topic.AiVenues[t].ChainGenerated)
                {
                    Console.WriteLineRed($"Removed {actionCount} chain generated pins");
                    topic.AiVenues.RemoveAt(t);
                    actionCount++;
                }
            }
        }
        if (actionCount > 0)
        {
            Console.WriteLineRed($"Removed {actionCount} chain generated pins");
        }
        _topicsService.SaveTopics(topics);
    }


    public void TagAiPinsWithNamesInSkipList(List<DetailedTopic> topics)
    {

        var restaurantService = new RestaurantTypeService();
        var invalid = 0;
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                var pin = venue.Pin;
                if (pin == null) continue;
                if (PlaceNameFilterHelper.IsInPlaceNameSkipList(venue.PlaceName))
                {
                    Console.WriteLineRed($"Tagging pin {t} in topic {i} - {venue.PlaceName}");
                    venue.IsExportable = false;
                    invalid++;
                }
            }
        }
        _topicsService.SaveTopics(topics);
    }


    public void TagAiPinsWithBadRestaurantTypes(List<DetailedTopic> topics)
    {
        var restaurantService = new RestaurantTypeService();
        var invalid = 0;
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                var pin = venue.Pin;
                if (pin == null) continue;
                var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude);
                if (cachePin == null) continue;
                if (restaurantService.IsRejectedRestaurantType(cachePin.MetaData?.RestaurantType))
                {
                    Console.WriteLineRed($"Tagging pin {t} in topic {i} - {cachePin.MetaData?.RestaurantType}");
                    venue.IsExportable = false;
                    invalid++;
                }
            }
        }
        _topicsService.SaveTopics(topics);
    }

}
