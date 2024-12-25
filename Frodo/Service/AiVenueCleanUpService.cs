// Ignore Spelling: Mis

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
using System.Web;

namespace Frodo.Service;

internal class AiVenueCleanUpService(
    GeoService _geoService,
    FBGroupService _fBGroupService,
    MapPinCache _mapPinCache,
    AiVenueLocationService _aiVenueLocationService,
    TopicsDataLoaderService _topicsService,
    IConsole Console)
{
    public void TagAiPinsFoundInDifferentCountry(List<DetailedTopic> topics)
    {
        LabelHelper.Reset();
        var invalidGeo = 0;
        var invalidGeoChain = 0;
        var invalidGeoChainGenerated = 0;
        var unmatchedLabels = 0;
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

                var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude, venue.PlaceName ?? "");
                if (cachePin == null) continue;

                if (!_aiVenueLocationService.IsPinInGroupCountry(pin, topic))
                {
                    var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                    if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                    var country = _geoService.GetCountryPin(cachePin);
                    if (venue.ChainGenerated)
                    {
                        invalidGeoChainGenerated++;
                    }
                    else if (venue.IsChain)
                    {
                        invalidGeoChain++;
                    }
                    else
                    {
                        invalidGeo++;
                    }
                    venue.IsExportable = false;
                    venue.InvalidGeo = true;
                }
            }
        }

        LabelHelper.Check();
        Console.WriteLine($"Unmatched labels {unmatchedLabels}");
        Console.WriteLine($"Invalid Geo pins {invalidGeo}");
        Console.WriteLine($"Invalid Chain Geo pins {invalidGeoChain}");
        Console.WriteLine($"Invalid Chain generated Geo pins {invalidGeoChainGenerated}");

        _topicsService.SaveTopics(topics);
    }

    public void TagAiPinsPermanentlyClosed(List<DetailedTopic> topics)
    {
        var permanentlyClosed = 0;
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

                var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude, venue.PlaceName ?? "");
                if (cachePin == null) continue;

                // Sync PermanentlyClosed status
                if (cachePin.MetaData != null && cachePin.MetaData.PermanentlyClosed && !venue.PermanentlyClosed)
                {
                    permanentlyClosed++;
                    venue.PermanentlyClosed = true;
                    venue.IsExportable = false;
                }
            }
        }

        Console.WriteLine($"Permanently Closed pins {permanentlyClosed}");
        _topicsService.SaveTopics(topics);
    }

    public void ResetIsExportable(List<DetailedTopic> topics)
    {
        Console.WriteLine("--------------------------------------");
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (i % 1000 == 0)
                Console.WriteLine($"{i} of {topics.Count}");

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                venue.IsExportable = true;
            }
        }

        _topicsService.SaveTopics(topics);
    }



    public void TagAiPinsNotFoundInOriginalText(List<DetailedTopic> topics)
    {
        LabelHelper.Reset();
        var unmatchedLabels = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

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
                            Console.WriteLine($"missing label in title, label :{venue.PlaceName} pin:{pin.Label}  :{topic.Title}");
                            // Wont work with ChainGenerated
                            //venue.IsExportable = false;

                            unmatchedLabels++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Tagging pin {t} in topic {i}");
                        //venue.IsExportable = false;
                        unmatchedLabels++;
                    }
                }

            }

        }

        Console.WriteLine("--------------------------------------");
        LabelHelper.Check();
        Console.WriteLine($"Unmatched labels {unmatchedLabels}");
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
                if (topic.Title.Contains('?')) noDataTopics.Add(topic.Title);
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
        int removedPinCount = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                if (PlaceNameFilterHelper.IsInPlaceNameSkipList(venue.PlaceName))
                {
                    Console.WriteLine($"Removing {venue.PlaceName}");
                    topic.AiVenues.RemoveAt(t);
                    removedPinCount++;
                }
            }
        }
        Console.WriteLine($"Removed pins due to generic names {removedPinCount}");
        _topicsService.SaveTopics(topics);
    }

    public void CheckForPlaceNamesPinMisMatch(List<DetailedTopic> topics)
    {
        Console.Clear();
        Console.WriteLine("--------------------------------------");
        int count = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];
            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];

                if (venue.Pin == null) continue;
                if (venue.ChainGenerated) continue;
                var label = PlaceNameAdjusterHelper.FixUserErrorsInPlaceNames(venue.Pin.Label ?? "", "", "");
                var placeName = PlaceNameAdjusterHelper.FixUserErrorsInPlaceNames(venue.PlaceName ?? "", "", "");

                label = HttpUtility.UrlDecode(label).Replace("’", "").Replace("'", "");
                placeName = HttpUtility.UrlDecode(placeName).Replace("’", "").Replace("'", "");

                if (!label.Contains(placeName, StringComparison.InvariantCultureIgnoreCase)
                    && !placeName.Contains(label, StringComparison.InvariantCultureIgnoreCase)
                    && !LabelHelper.IsInTextBlock(label, placeName))
                {
                    Console.WriteLine($"Found {placeName}, {label} ({venue.PlaceName})");
                    count++;
                    //venue.PinSearchDone = false;
                    //venue.Pin = null;
                }
            }
        }
        Console.WriteLine($"Total mismatched label names to place name {count}");
        _topicsService.SaveTopics(topics);
    }

    public void RemoveNullAiPins(List<DetailedTopic> topics)
    {
        Console.WriteLine("--------------------------------------");
        int removeCount = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                if (topic.AiVenues[t] == null)
                {
                    topic.AiVenues.RemoveAt(t);
                    removeCount++;
                }
            }
        }
        Console.WriteLine($"Removed null pins : {removeCount}");
        _topicsService.SaveTopics(topics);
    }

    public void RemoveCityPins(List<DetailedTopic> topics)
    {
        var citys = new CityService();
        Console.WriteLine("--------------------------------------");
        int removeCount = 0;
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                if (citys.IsCity(topic.AiVenues[t].PlaceName))
                {
                    Console.WriteLineRed($"Removing city place name : {topic.AiVenues[t].PlaceName}");
                    topic.AiVenues.RemoveAt(t);
                    removeCount++;
                }
            }
        }
        Console.WriteLine($"Removed city place name : {removeCount}");
        _topicsService.SaveTopics(topics);
    }

    public List<string> GetPlaceNames(List<DetailedTopic> topics)
    {
        var citys = new CityService();
        Console.WriteLine("--------------------------------------");
        List<string> placeNames = [];
        for (int i = 0; i < topics.Count; i++)
        {
            DetailedTopic? topic = topics[i];

            if (topic.AiVenues == null) continue;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                if (topic.AiVenues[t].Pin?.Label != null && !placeNames.Contains(topic.AiVenues[t].Pin.Label))
                {
                    placeNames.Add(topic.AiVenues[t].Pin.Label);
                }
            }
        }

        placeNames.Sort();
        return placeNames;
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


    public void TagGenericPlaceNames(List<DetailedTopic> topics)
    {
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
                if (PlaceNameFilterHelper.StartsWithPlaceNameSkipList(venue.PlaceName))
                {
                    venue.IsExportable = false;
                    invalid++;
                }
            }
        }
        Console.WriteLineRed($"Pins in Skip list {invalid}");
        _topicsService.SaveTopics(topics);
    }

    public void CountPins(List<DetailedTopic> Topics)
    {
        int count = 0;
        int notExportable = 0;
        int notExportableChain = 0;
        int chain = 0;
        int chainGenerated = 0;
        int rejectedRestaurantType = 0;
        int permanentlyClosed = 0;
        int nullPins = 0;
        int validPins = 0;
        int describeCount = 0;
        int questionCount = 0;
        int unknownCount = 0;
        int emptyCount = 0;
        //string QuestionLinesWithoutQ = "";
        //string DescribeWithQ = "";
        //string UnknownWithQ = "";
        //string UnknownWithoutQ = "";

        for (int i = 0; i < Topics.Count; i++)
        {
            DetailedTopic? topic = Topics[i];
            if (topic.TitleCategory == "DESCRIBE")
            {
                //if (topic.Title.Contains("?")) DescribeWithQ += topic.Title + "\r\n";
                describeCount++;
            }
            else if (topic.TitleCategory == "QUESTION")
            {
                //if (!topic.Title.Contains("?")) QuestionLinesWithoutQ += topic.Title + "\r\n";
                questionCount++;
            }
            else if (topic.TitleCategory == "UNKNOWN")
            {
                //if (topic.Title.Contains("?")) UnknownWithQ += topic.Title + "\r\n";
                //if (!topic.Title.Contains("?")) UnknownWithoutQ += topic.Title + "\r\n";
                unknownCount++;
            }

            else emptyCount++;

            if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

            foreach (var ai in topic.AiVenues)
            {
                if (!ai.IsExportable)
                {
                    if (!ai.RejectedRestaurantType && !ai.PermanentlyClosed)
                    {
                        if (ai.ChainGenerated) notExportableChain++;
                        else notExportable++;
                    }
                }
                if (ai.IsExportable && !ai.ChainGenerated && !ai.IsChain && !ai.PermanentlyClosed && !ai.RejectedRestaurantType) count++;

                if (ai.IsChain) chain++;
                if (ai.ChainGenerated) chainGenerated++;
                if (ai.PermanentlyClosed) permanentlyClosed++;
                if (ai.RejectedRestaurantType) rejectedRestaurantType++;
                if (ai.Pin == null && !ai.IsChain) nullPins++;
                if (ai.Pin != null && ai.IsExportable) validPins++;
            }
        }
        //string filePath = "D:\\Coding\\Gluten\\Database\\";
        //File.WriteAllText(filePath + "QuestionLinesWithoutQ.txt", QuestionLinesWithoutQ);
        //File.WriteAllText(filePath + "DescribeWithQ.txt", DescribeWithQ);
        //File.WriteAllText(filePath + "UnknownWithQ.txt", UnknownWithQ);
        //File.WriteAllText(filePath + "UnknownWithoutQ.txt", UnknownWithoutQ);

        Console.WriteLine($"Total pin count (valid/not chain) : {count}");
        Console.WriteLine($"Total not Exportable : {notExportable}");
        Console.WriteLine($"Total not Exportable Chain: {notExportableChain}");
        Console.WriteLine($"Total chain : {chain}");
        Console.WriteLine($"Total chain Generated : {chainGenerated}");
        Console.WriteLine($"Total rejected Restaurant Type : {rejectedRestaurantType}");
        Console.WriteLine($"Total permanently Closed : {permanentlyClosed}");
        Console.WriteLine($"Total null Pins : {nullPins}");
        Console.WriteLine($"Total valid : {validPins}");

        Console.WriteLine($"----- categorisation -----");
        Console.WriteLine($"describe count : {describeCount}");
        Console.WriteLine($"question count : {questionCount}");
        Console.WriteLine($"unknown count : {unknownCount}");
        Console.WriteLine($"empty count : {emptyCount}");

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
                var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude, venue.PlaceName ?? "");
                if (cachePin == null) continue;
                if (restaurantService.IsRejectedRestaurantType(cachePin.MetaData?.RestaurantType))
                {
                    venue.IsExportable = false;
                    venue.RejectedRestaurantType = true;
                    invalid++;
                }
            }
        }
        Console.WriteLineRed($"Pins with invalid restaurant types :{invalid}");
        _topicsService.SaveTopics(topics);
    }

}
