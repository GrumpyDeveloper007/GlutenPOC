﻿using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.TopicModel;
using Gluten.Data.Access.Service;
using Gluten.Data.Access.DatabaseModel;
using Gluten.Core.Interface;
using Gluten.Core.Helper;
using System.Diagnostics;


namespace Frodo.Service
{
    /// <summary>
    /// Generates and exports data to the client database
    /// </summary>
    internal class ClientExportGeneratorService(DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        MapPinCacheService _mapPinCache,
        FBGroupService _fBGroupService,
        GeoService _geoService,
        CloudDataStoreService _dataStore,
        AiInterfaceService _analyzeDocumentService,
        IConsole Console
        )
    {
        private const string ExportFolder = @"D:\Coding\Gluten\Export\";
        private readonly ClientExportGeneratorGMService _exportFileGeneratorGM = new(_databaseLoaderService, _geoService, _dataStore, Console);

        /// <summary>
        /// Group data by pin (venue), export to json
        /// </summary>
        public async Task GenerateTopicExport(List<DetailedTopic> topics)
        {
            List<PinTopic>? pins = [];// _databaseLoaderService.LoadPinTopics();
            pins ??= [];


            TopicListHelper.CleanTopics(pins);
            pins = ExtractPinExport(pins, topics, _mappingService);
            TopicListHelper.RemoveEmptyPins(pins);

            var ii = 0;
            var unknownRestaurantType = 0;
            var itemsUpdated = 0;
            Stopwatch timer = new();

            foreach (var pin in pins)
            {
                if (itemsUpdated > 10)
                {
                    _databaseLoaderService.SavePinTopics(pins);
                    itemsUpdated = 0;
                }

                if (pin.Price != null)
                {
                    pin.Price = pin.Price.Replace("&nbsp;", " ");
                    pin.Price = pin.Price.Replace("·", "");
                }
                pin.Label = (pin.Label ?? "").Replace("&amp;", "&");
                pin.Label = pin.Label.Replace("+", " ");
                pin.Label = pin.Label.Replace("%27", "'");


                if (string.IsNullOrEmpty(pin.Description)
                    || pin.Description?.Contains(pin.Label ?? "", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    var message = "";
                    foreach (var item in pin.Topics)
                    {
                        message += " " + item.Title;
                    }

                    var existingCache = _databaseLoaderService.GetPinDescriptionCache(pin.GeoLongitude, pin.GeoLatitude);
                    if (existingCache == null)//|| pin.Description?.Contains(pin.Label ?? "") == false
                    {
                        Console.WriteLine("--------------------------------------");
                        Console.WriteLine($"Updating descriptions - {ii} of {pins.Count} location:{pin.GeoLongitude}:{pin.GeoLatitude}");
                        timer.Restart();
                        pin.Description = await _analyzeDocumentService.ExtractDescriptionTitle(message, pin.Label);
                        timer.Stop();
                        Console.WriteLine($"Description created time:{timer.Elapsed.TotalSeconds} ");

                        itemsUpdated++;
                        if (!string.IsNullOrWhiteSpace(pin.Description))
                        {
                            _databaseLoaderService.AddPinDescriptionCache(pin.Description, pin.GeoLongitude, pin.GeoLatitude);
                            _databaseLoaderService.SavePinDescriptionCache();
                        }
                    }
                    else
                    {
                        pin.Description = existingCache.Description;
                    }
                }



                if (string.IsNullOrWhiteSpace(pin.RestaurantType))
                {
                    unknownRestaurantType++;
                }

                pin.Description = $"AI Generated : {pin.Description}";

                ii++;
            }
            TopicListHelper.RemoveTopicTitles(pins);

            WriteToDatabase(pins);

            _databaseLoaderService.SavePinTopics(pins);
            CreateExportFolderData(pins, topics);
            _exportFileGeneratorGM.GenerateTopicExport(pins, ExportFolder);
            Console.WriteLine($"Unknown restaurant types : {unknownRestaurantType}");
        }

        private List<PinTopic> ExtractPinExport(List<PinTopic> pins, List<DetailedTopic> topics, MappingService mapper)
        {
            foreach (var topic in topics)
            {
                var newT = mapper.Map<PinLinkInfo, DetailedTopic>(topic);
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                if (topic.AiVenues != null)
                {
                    bool resetChains = false;
                    for (int i = topic.AiVenues.Count - 1; i >= 0; i--)
                    {
                        AiVenue? aiVenue = topic.AiVenues[i];
                        if (aiVenue.Pin != null && aiVenue.IsExportable && !aiVenue.IsChain)
                        {
                            var cachePin = _mapPinCache.TryGetPinLatLong(aiVenue.Pin.GeoLatitude, aiVenue.Pin.GeoLongitude, aiVenue.PlaceName);
                            cachePin ??= _mapPinCache.TryGetPinLatLong(aiVenue.Pin.GeoLatitude, aiVenue.Pin.GeoLongitude, aiVenue.Pin.Label ?? "");
                            if (cachePin == null)
                            {
                                if (aiVenue.ChainGenerated)
                                {
                                    Console.WriteLine($"Unable to get cache pin, broken chain generated : {aiVenue.PlaceName} ");
                                    resetChains = true;
                                    //topic.AiVenues.RemoveAt(i);
                                    continue;
                                }
                                else
                                {
                                    Console.WriteLine($"Unable to get cache pin, broken static pin {aiVenue.PlaceName} ");
                                    //aiVenue.Pin = null;
                                    aiVenue.PinSearchDone = false;
                                    continue;
                                }
                            }

                            var pinCountry = _geoService.GetCountryPin(cachePin);

                            if (!groupCountry.Contains(pinCountry, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(groupCountry))
                            {
                                Console.WriteLine($"Rejecting pin for {groupCountry}, pin country : {pinCountry}");
                                continue;
                            }

                            if (aiVenue.Pin != null && !_fBGroupService.IsPinWithinExpectedRange(topic.GroupId, aiVenue.Pin))
                            {
                                Console.WriteLine($"Rejecting pin latitude / longitude out of range");
                                continue;
                            }


                            if (aiVenue.Pin != null
                                && !string.IsNullOrWhiteSpace(aiVenue.Pin.GeoLatitude)
                                && !string.IsNullOrWhiteSpace(aiVenue.Pin.GeoLongitude)
                                )
                            {
                                var existingPin = TopicListHelper.IsPinInList(aiVenue.Pin, pins);
                                TopicListHelper.AddIfNotExists(pins, existingPin, newT, aiVenue.Pin, cachePin);
                            }
                        }
                    }

                    if (resetChains)
                    {
                        TopicActionHelper.RemoveChainGeneratedAiPins(topic);
                    }
                }

                if (topic.UrlsV2 != null)
                {
                    foreach (var url in topic.UrlsV2)
                    {
                        if (url.Pin != null)
                        {
                            var cachePin = _mapPinCache.TryGetPinLatLong(url.Pin.GeoLatitude, url.Pin.GeoLongitude, "");
                            if (cachePin == null)
                            {
                                Console.WriteLine($"Unable to get cache pin {url.Pin.Label}");
                            }

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
                                var existingPin = TopicListHelper.IsPinInList(url.Pin, pins);
                                TopicListHelper.AddIfNotExists(pins, existingPin, newT, url.Pin, cachePin);
                            }
                        }
                    }
                }
            }
            return pins;
        }

        private static void SaveDb<typeToSave>(string fileName, typeToSave topics)
        {
            JsonHelper.SaveDbNoPadding(fileName, topics);
        }

        private static PinTopicDb? FindDbPin(List<PinTopicDb> dbPins, PinTopic gMapsPin)
        {
            foreach (var pin in dbPins)
            {
                if (pin.GeoLatitude == gMapsPin.GeoLatitude && pin.GeoLongitude == gMapsPin.GeoLongitude)
                {
                    return pin;
                }
            }
            return null;
        }

        private void WriteToDatabase(List<PinTopic> pins)
        {
            var mapper = new DbMapperService();

            // delete locally removed
            var items = _dataStore.GetData<PinTopicDb>("").Result;
            var itemsToDelete = 0;
            var itemsToAdd = 0;
            var itemsToUpdate = 0;
            Dictionary<string, int> deleteByCountry = [];
            Dictionary<string, int> updateByCountry = [];
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!pins.Exists(o => o.GeoLatitude == item.GeoLatitude && o.GeoLongitude == item.GeoLongitude))
                {
                    Console.WriteLine($"Marked for deletion : {item.Label}, {item.Country}");
                    deleteByCountry.TryAdd(item.Country, 0);
                    deleteByCountry[item.Country] += 1;
                    itemsToDelete++;
                }
            }

            for (int i = 0; i < pins.Count; i++)
            {
                var item = pins[i];
                var existingDbPin = items.SingleOrDefault(o => o.GeoLatitude == item.GeoLatitude && o.GeoLongitude == item.GeoLongitude);
                if (existingDbPin != null)
                {
                    if (item.Label != existingDbPin.Label
                        || item.Description != existingDbPin.Description
                        || item.MapsLink != existingDbPin.MapsLink
                        || item.RestaurantType != existingDbPin.RestaurantType
                        || item.Price != existingDbPin.Price
                        || item.Stars != existingDbPin.Stars
                        || item.Topics.Count != existingDbPin.Topics.Count)
                    {
                        itemsToUpdate++;
                        updateByCountry.TryAdd(existingDbPin.Country, 0);
                        updateByCountry[existingDbPin.Country] += 1;

                    }

                }
                else
                {
                    itemsToAdd++;
                }
            }

            foreach (var item in deleteByCountry)
            {
                Console.WriteLine($"Items to Delete : {item.Key} = {item.Value}");
            }
            Console.WriteLine($"Items to Add : {itemsToAdd}");
            Console.WriteLine($"Items to Delete : {itemsToDelete}");
            Console.WriteLine($"Items to Update : {itemsToUpdate}");



            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!pins.Exists(o => o.GeoLatitude == item.GeoLatitude && o.GeoLongitude == item.GeoLongitude))
                {
                    if (i % 10 == 0)
                        Console.WriteLine($"Delete PinTopic {i}");
                    _dataStore.DeleteItemAsync(item).Wait();
                }
            }

            for (int i = 0; i < pins.Count; i++)
            {
                var item = pins[i];
                var existingDbPin = FindDbPin(items, item);
                if (i % 500 == 0)
                    Console.WriteLine($"Writing to database {i}");
                var dbItem = mapper.Map<PinTopicDb, PinTopic>(item);
                dbItem.Country = _geoService.GetCountryPin(item);

                var topicsDifferent = false;
                if (existingDbPin != null && item.Topics.Count == existingDbPin.Topics.Count)
                {
                    for (int t = 0; t < item.Topics.Count; t++)
                    {
                        PinLinkInfo? topic = item.Topics[t];
                        var existingTopic = existingDbPin.Topics[t];
                        if (topic.FacebookUrl != existingTopic.FacebookUrl
                            || topic.NodeID != existingTopic.NodeID
                            || topic.Title != existingTopic.Title
                            || topic.ShortTitle != existingTopic.ShortTitle
                            || topic.PostCreated != existingTopic.PostCreated)
                        {
                            topicsDifferent = true;
                        }
                    }
                }

                if (existingDbPin == null
                    || item.Label != existingDbPin.Label
                    || item.Description != existingDbPin.Description
                    || item.MapsLink != existingDbPin.MapsLink
                    || item.RestaurantType != existingDbPin.RestaurantType
                    || item.Price != existingDbPin.Price
                    || item.Stars != existingDbPin.Stars
                    || item.Topics.Count != existingDbPin.Topics.Count
                    || topicsDifferent)
                {
                    _dataStore.ReplaceItemAsync(dbItem).Wait();
                }
            }

        }

        private void CreateExportFolderData(List<PinTopic> pins, List<DetailedTopic> topics)
        {
            Dictionary<string, List<PinTopic>> files = [];

            foreach (var item in pins)
            {
                string groupId = "";
                string groupCountry = "";
                if (item.Topics != null)
                {
                    foreach (var topic in item.Topics)
                    {
                        var selectedTopic = topics.First(o => o.NodeID == topic.NodeID);
                        groupId = selectedTopic.GroupId;
                        groupCountry = _fBGroupService.GetCountryName(groupId);
                        if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = selectedTopic.TitleCountry ?? "";
                        break;
                    }
                }


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

    }
}
