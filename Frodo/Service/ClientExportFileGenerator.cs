using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.PinDescription;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class ClientExportFileGenerator(DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        PinHelper _pinHelper)
    {
        private readonly LocalAiInterfaceService _analyzeDocumentService = new();


        /// <summary>
        /// Group data by pin (venue), export to json
        /// </summary>
        public void GenerateTopicExport(List<DetailedTopic> topics)
        {
            List<PinTopic>? pins = _databaseLoaderService.LoadPinTopics();
            pins ??= [];

            DataHelper.CleanTopics(pins);
            pins = ExtractPinExport(pins, topics, _mappingService);
            DataHelper.RemoveEmptyPins(pins);

            var ii = 0;
            var unknownRestaurantType = 0;
            foreach (var pin in pins)
            {
                if (string.IsNullOrEmpty(pin.Description))
                {
                    var message = "";
                    if (pin.Topics != null)
                    {
                        List<string> nodes = [];
                        foreach (var item in pin.Topics)
                        {
                            message += " " + item.Title;
                            if (item.NodeID != null)
                                nodes.Add(item.NodeID);
                        }

                        var existingCache = _databaseLoaderService.GetPinDescriptionCache(nodes);
                        if (existingCache == null)
                        {

                            pin.Description = _analyzeDocumentService.ExtractDescriptionTitle(message, pin.Label);
                            var cache = new PinDescriptionCache()
                            {
                                Nodes = nodes,
                                Description = pin.Description
                            };
                            _databaseLoaderService.AddPinDescriptionCache(cache);
                            _databaseLoaderService.SavePinDescriptionCache();
                        }
                        else
                        {
                            pin.Description = existingCache.Description;
                        }
                        _databaseLoaderService.SavePinTopics(pins);

                    }
                }

                if (string.IsNullOrWhiteSpace(pin.RestaurantType))
                {
                    unknownRestaurantType++;
                }

                Console.WriteLine($"Updating descriptions - {ii} of {pins.Count}");
                ii++;
            }
            DataHelper.RemoveTopicTitles(pins);


            _databaseLoaderService.SavePinTopics(pins);
            Console.WriteLine($"Unknown restaurant types : {unknownRestaurantType}");
        }

        private List<PinTopic> ExtractPinExport(List<PinTopic> pins, List<DetailedTopic> topics, MappingService mapper)
        {
            foreach (var topic in topics)
            {
                var newT = mapper.Map<PinLinkInfo, DetailedTopic>(topic);

                if (topic.GroupId != FBGroupService.DefaultGroupId) continue;

                if (topic.AiVenues != null)
                {
                    foreach (var aiVenue in topic.AiVenues)
                    {
                        if (aiVenue.Pin != null
                            && !string.IsNullOrWhiteSpace(aiVenue.Pin.GeoLatitude)
                            && !string.IsNullOrWhiteSpace(aiVenue.Pin.GeoLongitude)
                            && FBGroupService.IsPinWithinExpectedRange(topic.GroupId, double.Parse(aiVenue.Pin.GeoLatitude), double.Parse(aiVenue.Pin.GeoLongitude)))
                        {
                            var existingPin = DataHelper.IsPinInList(aiVenue.Pin, pins);
                            var cachePin = _pinHelper.TryGetPin(aiVenue.Pin.Label);
                            DataHelper.AddIfNotExists(pins, existingPin, newT, aiVenue.Pin, cachePin);
                        }
                    }
                }

                if (topic.UrlsV2 != null)
                {
                    foreach (var url in topic.UrlsV2)
                    {

                        if (url.Pin != null
                            && !string.IsNullOrWhiteSpace(url.Pin.GeoLatitude)
                            && !string.IsNullOrWhiteSpace(url.Pin.GeoLongitude)
                            && FBGroupService.IsPinWithinExpectedRange(topic.GroupId, double.Parse(url.Pin.GeoLatitude), double.Parse(url.Pin.GeoLongitude)))
                        {
                            var existingPin = DataHelper.IsPinInList(url.Pin, pins);
                            var cachePin = _pinHelper.TryGetPin(url.Pin.Label);
                            DataHelper.AddIfNotExists(pins, existingPin, newT, url.Pin, cachePin);
                        }
                    }
                }
            }
            return pins;
        }




    }
}
