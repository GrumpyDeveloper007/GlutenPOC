using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.MapsModel;
using Gluten.Data.PinDescription;
using Gluten.Data.TopicModel;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class ClientExportFileGenerator(DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        MapPinCache _mapPinCache,
        FBGroupService _fBGroupService
        )
    {
        private readonly LocalAiInterfaceService _analyzeDocumentService = new();
        private const string ExportFolder = @"D:\Coding\Gluten\Export\";

        private static void SaveDb<typeToSave>(string fileName, typeToSave topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.None,
                [new StringEnumConverter()]);
            File.WriteAllText(fileName, json);
        }

        private void CreateExportFolderData(List<PinTopic> pins, List<DetailedTopic> topics)
        {
            Dictionary<string, List<PinTopic>> files = new();
            var fbGroupService = new FBGroupService();

            foreach (var item in pins)
            {
                string groupId = "";
                if (item.Topics != null)
                {
                    foreach (var topic in item.Topics)
                    {
                        var selectedTopic = topics.First(o => o.NodeID == topic.NodeID);
                        groupId = selectedTopic.GroupId;
                        break;
                    }
                }

                var groupCountry = fbGroupService.GetCountryName(groupId);

                if (!files.ContainsKey(groupCountry))
                {
                    files.Add(groupCountry, new List<PinTopic>());
                }

                files[groupCountry].Add(item);
            }

            foreach (var file in files)
            {
                SaveDb(ExportFolder + file.Key + ".json", file.Value);
            }
        }


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
                    Console.WriteLine($"Updating descriptions - {ii} of {pins.Count}");
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

                ii++;
            }
            DataHelper.RemoveTopicTitles(pins);


            _databaseLoaderService.SavePinTopics(pins);

            GenerateGMPinExport(pins);
            CreateExportFolderData(pins, topics);
            Console.WriteLine($"Unknown restaurant types : {unknownRestaurantType}");
        }

        private void GenerateGMPinExport(List<PinTopic> pins)
        {
            var gmPins = _databaseLoaderService.LoadGMPins();
            List<GMapsPin> exportPins = new();

            foreach (var pin in gmPins)
            {
                if (!string.IsNullOrWhiteSpace(pin.Comment)
                    && pin.GeoLatitude != null
                    && pin.GeoLongitude != null
                    && !IsInList(pins, double.Parse(pin.GeoLatitude), double.Parse(pin.GeoLongitude)))
                {
                    pin.Description = $"Pin generated from Google maps - {pin.Comment}";
                    exportPins.Add(pin);
                }
            }

            _databaseLoaderService.SaveGMMapPinExport(exportPins);
            Console.WriteLine($"Exported {exportPins.Count} pins generated by google maps ");
        }

        private bool IsInList(List<PinTopic> pins, double geoLatitude, double geoLongitude)
        {
            foreach (var pin in pins)
            {
                if (pin.GeoLatitude == geoLatitude && pin.GeoLongitude == geoLongitude) { return true; }
            }
            return false;
        }

        private List<PinTopic> ExtractPinExport(List<PinTopic> pins, List<DetailedTopic> topics, MappingService mapper)
        {
            foreach (var topic in topics)
            {
                var newT = mapper.Map<PinLinkInfo, DetailedTopic>(topic);
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);

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
                            var cachePin = _mapPinCache.TryGetPin(aiVenue.Pin.Label, groupCountry);
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
                            var cachePin = _mapPinCache.TryGetPin(url.Pin.Label, groupCountry);
                            DataHelper.AddIfNotExists(pins, existingPin, newT, url.Pin, cachePin);
                        }
                    }
                }
            }
            return pins;
        }




    }
}
