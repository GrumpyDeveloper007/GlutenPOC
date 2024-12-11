using Frodo.Helper;
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
    public void RemoveAiPinsInBadLocations(List<DetailedTopic> Topics)
    {
        Console.Clear();
        LabelHelper.Reset();
        var invalidGeo = 0;
        var unmatchedLabels = 0;
        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (i % 100 == 0)
                Console.WriteLine($"Looking for invalid AI pins {i} of {Topics.Count}");

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                var pin = venue.Pin;
                if (venue.PlaceName == "Vegan restaurant")
                {
                    topic.AiVenues.RemoveAt(t);
                }

                // Try to filter out invalid pins
                if (!LabelHelper.IsInTextBlock(venue.PlaceName, topic.Title))
                {
                    if (pin != null)
                    {
                        if (!LabelHelper.IsInTextBlock(pin.Label, topic.Title))
                        {
                            //Console.WriteLine($"Removing pin {t} in topic {i}");
                            Console.WriteLine($"missing label in title, label :{venue.PlaceName} pin:{pin.Label}  :{topic.Title}");
                            //topic.AiVenues.RemoveAt(t);
                            unmatchedLabels++;
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"missing label in title, label :{venue.PlaceName}  :{topic.Title}");
                        Console.WriteLine($"Removing pin {t} in topic {i}");
                        //topic.AiVenues.RemoveAt(t);
                        unmatchedLabels++;
                    }
                }

                if (pin == null) continue;

                var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude);
                if (cachePin == null) continue;

                if (!_aiVenueLocationService.IsPinInGroupCountry(pin, topic))
                {
                    var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                    if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                    var country = _geoService.GetCountryPin(cachePin);
                    invalidGeo++;
                    Console.WriteLine($"Removing pin {t} in topic {i} - country mismatch {groupCountry}, {country}");
                    topic.AiVenues.RemoveAt(t);
                    continue;
                }
                if (cachePin.MetaData != null && cachePin.MetaData.PermanentlyClosed)
                {
                    Console.WriteLine($"Removing pin {t} in topic {i} - PermanentlyClosed");
                    topic.AiVenues.RemoveAt(t);
                }
            }

        }

        LabelHelper.Check();
        Console.WriteLine($"Unmatched labels {unmatchedLabels}");
        Console.WriteLine($"Invalid pins {invalidGeo}");
        _topicsService.SaveTopics(Topics);
    }

    public void RemoveAiPinsWithBadRestaurantTypes(List<DetailedTopic> Topics)
    {
        var restaurantService = new RestaurantTypeService();
        var invalid = 0;
        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (i % 100 == 0)
                Console.WriteLine($"Processing AI pins {i} of {Topics.Count}");

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
                    Console.WriteLine($"Removing pin {t} in topic {i} - {cachePin.MetaData?.RestaurantType}");
                    topic.AiVenues.RemoveAt(t);
                    invalid++;
                }
            }
        }
        _topicsService.SaveTopics(Topics);
    }

}
