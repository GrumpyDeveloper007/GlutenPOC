using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.MapsModel;
using Gluten.Data.PinDescription;
using Gluten.Data.TopicModel;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Gluten.Data.Access.Service;
using Gluten.Data.Access.DatabaseModel;
using Gluten.Core.Helper;

namespace Frodo.Service
{
    internal class ClientExportFileGenerator(DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        MapPinCache _mapPinCache,
        FBGroupService _fBGroupService,
        GeoService _geoService,
        CloudDataStore _dataStore
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

        private void WriteToDatabase(List<PinTopic> pins, List<GMapsPin> gmPins)
        {
            var mapper = new DbMapper();
            //_dataStore.DeleteContainer<PinTopicDb>().Wait();
            //_dataStore.DeleteContainer<GMapsPinDb>().Wait();

            // delete locally removed
            var itemsGm = _dataStore.GetData<GMapsPinDb>("").Result;
            for (int i = 0; i < itemsGm.Count; i++)
            {
                var item = itemsGm[i];
                if (!gmPins.Exists(o => o.GeoLatitude == item.GeoLatitude && o.GeoLongitude == item.GeoLongitude))
                {
                    Console.WriteLine($"Delete item {i}");
                    _dataStore.DeleteItemAsync(item).Wait();
                }
            }

            var items = _dataStore.GetData<PinTopicDb>("").Result;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!pins.Exists(o => o.GeoLatitude == item.GeoLatitude && o.GeoLongitude == item.GeoLongitude))
                {
                    Console.WriteLine($"Delete item {i}");
                    _dataStore.DeleteItemAsync(item).Wait();
                }
            }

            for (int i = 0; i < gmPins.Count; i++)
            {
                var item = gmPins[i];
                item.Description = $"Pin generated from Google maps - {item.Comment}";
                Console.WriteLine($"Writing to database {i}");
                var dbItem = mapper.Map<GMapsPinDb, GMapsPin>(item);
                dbItem.Country = _geoService.GetCountryPin(item);
                _dataStore.ReplaceItemAsync(dbItem).Wait();
            }


            for (int i = 0; i < pins.Count; i++)
            {
                var item = pins[i];
                Console.WriteLine($"Writing to database {i}");
                var dbItem = mapper.Map<PinTopicDb, PinTopic>(item);
                dbItem.Country = _geoService.GetCountryPin(item);
                _dataStore.ReplaceItemAsync(dbItem).Wait();
            }

        }


        private void CreateExportFolderData(List<PinTopic> pins, List<DetailedTopic> topics)
        {
            Dictionary<string, List<PinTopic>> files = [];

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

                var groupCountry = _fBGroupService.GetCountryName(groupId);

                if (!files.TryGetValue(groupCountry, out List<PinTopic>? value))
                {
                    value = ([]);
                    files.Add(groupCountry, value);
                }

                value.Add(item);
            }

            foreach (var file in files)
            {
                SaveDb(ExportFolder + file.Key + ".json", file.Value);
            }
        }

        private void CreateExportFolderDataGM(List<GMapsPin> pins)
        {
            Dictionary<string, List<GMapsPin>> files = [];

            foreach (var item in pins)
            {
                var pinCountry = _geoService.GetCountryPin(item);

                if (!string.IsNullOrWhiteSpace(pinCountry))
                {
                    if (!files.TryGetValue(pinCountry, out List<GMapsPin>? value))
                    {
                        value = ([]);
                        files.Add(pinCountry, value);
                    }

                    value.Add(item);
                }
            }

            foreach (var file in files)
            {
                SaveDb(ExportFolder + file.Key + "GM.json", file.Value);
            }
        }


        /// <summary>
        /// Group data by pin (venue), export to json
        /// </summary>
        public void GenerateTopicExport(List<DetailedTopic> topics)
        {
            List<PinTopic>? pins = _databaseLoaderService.LoadPinTopics();
            pins ??= [];
            var gmPins = _databaseLoaderService.LoadGMPins();

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

            GenerateGMPinExport(pins);

            var gmPinsWithDescriptions = gmPins.Where(x => !string.IsNullOrWhiteSpace(x.Comment) && x.Comment.Contains("gluten", StringComparison.InvariantCultureIgnoreCase)).ToList();
            WriteToDatabase(pins, gmPinsWithDescriptions);

            _databaseLoaderService.SavePinTopics(pins);
            CreateExportFolderData(pins, topics);
            CreateExportFolderDataGM(gmPins);
            Console.WriteLine($"Unknown restaurant types : {unknownRestaurantType}");
        }

        private void GenerateGMPinExport(List<PinTopic> pins)
        {
            var gmPins = _databaseLoaderService.LoadGMPins();
            List<GMapsPin> exportPins = [];

            foreach (var pin in gmPins)
            {
                if (!string.IsNullOrWhiteSpace(pin.Comment)
                    && pin.GeoLatitude != null
                    && pin.GeoLongitude != null
                    && !PinHelper.IsInList(pins, double.Parse(pin.GeoLatitude), double.Parse(pin.GeoLongitude)))
                {
                    pin.Description = $"Pin generated from Google maps - {pin.Comment}";
                    exportPins.Add(pin);
                }
            }

            _databaseLoaderService.SaveGMMapPinExport(exportPins);
            Console.WriteLine($"Exported {exportPins.Count} pins generated by google maps ");
        }

        private List<PinTopic> ExtractPinExport(List<PinTopic> pins, List<DetailedTopic> topics, MappingService mapper)
        {
            foreach (var topic in topics)
            {
                var newT = mapper.Map<PinLinkInfo, DetailedTopic>(topic);
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);

                if (_fBGroupService.IsGenericGroup(topic.GroupId)) continue; // skip export for generic groups 

                if (topic.AiVenues != null)
                {
                    foreach (var aiVenue in topic.AiVenues)
                    {
                        if (aiVenue.Pin != null)
                        {

                            var cachePin = _mapPinCache.TryGetPin(aiVenue.Pin.Label, groupCountry);
                            var pinCountry = _geoService.GetCountryPin(cachePin);

                            if (!groupCountry.Contains(pinCountry, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(groupCountry))
                            {
                                Console.WriteLine($"Rejecting pin for {groupCountry}, pin country : {pinCountry}");
                                continue;
                            }

                            if (!_fBGroupService.IsPinWithinExpectedRange(topic.GroupId, aiVenue.Pin))
                            {
                                Console.WriteLine($"Rejecting pin latitude / longitude out of range");
                                continue;
                            }


                            if (aiVenue.Pin != null
                                && !string.IsNullOrWhiteSpace(aiVenue.Pin.GeoLatitude)
                                && !string.IsNullOrWhiteSpace(aiVenue.Pin.GeoLongitude)
                                //&& FBGroupService.IsPinWithinExpectedRange(topic.GroupId, double.Parse(aiVenue.Pin.GeoLatitude), double.Parse(aiVenue.Pin.GeoLongitude))
                                )
                            {
                                var existingPin = DataHelper.IsPinInList(aiVenue.Pin, pins);
                                DataHelper.AddIfNotExists(pins, existingPin, newT, aiVenue.Pin, cachePin);
                            }
                        }
                    }
                }

                if (topic.UrlsV2 != null)
                {
                    foreach (var url in topic.UrlsV2)
                    {
                        if (url.Pin != null)
                        {
                            var cachePin = _mapPinCache.TryGetPin(url.Pin.Label, groupCountry);
                            var pinCountry = _geoService.GetCountryPin(cachePin);

                            if (!groupCountry.Contains(pinCountry, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(groupCountry))
                            {
                                Console.WriteLine($"Rejecting pin for {groupCountry}, pin country : {pinCountry}");
                                continue;
                            }

                            if (url.Pin != null
                                && !string.IsNullOrWhiteSpace(url.Pin.GeoLatitude)
                                && !string.IsNullOrWhiteSpace(url.Pin.GeoLongitude)
                                //&& FBGroupService.IsPinWithinExpectedRange(topic.GroupId, double.Parse(url.Pin.GeoLatitude), double.Parse(url.Pin.GeoLongitude))
                                )
                            {
                                var existingPin = DataHelper.IsPinInList(url.Pin, pins);
                                DataHelper.AddIfNotExists(pins, existingPin, newT, url.Pin, cachePin);
                            }
                        }
                    }
                }
            }
            return pins;
        }




    }
}
