using Frodo.Helper;
using Gluten.Core.DataProcessing.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.MapsModel;
using Gluten.Data.TopicModel;


namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// All the magic happens here
    /// </summary>
    internal class DataSyncService(MapPinService _mapPinService,
        DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        ClientExportGeneratorService _clientExportFileGenerator,
        GeoService _geoService,
        FBGroupService _fBGroupService,
        MapPinCacheService _mapPinCache,
        PinCacheSyncService _pinCacheSyncService,
        CityService _cityService,
        AiVenueLocationService _aiVenueLocationService,
        TopicsDataLoaderService _topicsLoaderService,
        AiVenueCleanUpService _aiVenueCleanUpService,
        AiInterfaceService _localAi,
        AiVenueProcessorService _aiVenueProcessorService,
        IConsole Console
        )
    {
        private readonly string _responsefileName = "D:\\Coding\\Gluten\\Database\\Responses.txt";
        private readonly TopicLoaderService _topicLoaderService = new(_topicsLoaderService, _fBGroupService, Console);
        private readonly TopicEmbeddedLinkService _topicEmbeddedLinkService = new(_databaseLoaderService, _mapPinService, _fBGroupService, _mappingService, _topicsLoaderService);
        private readonly AiVenueCreationService _aiVenueCreationService = new(_topicsLoaderService, _localAi, Console);
        private readonly TopicMetaCreationService _topicMetaCreationService = new(_topicsLoaderService, _localAi, _geoService, _cityService, _fBGroupService, Console);
        private List<GMapsPin> _gmSharedPins = [];

        public List<DetailedTopic> Topics = [];

        /// <summary>
        /// Processes the file generated from FB, run through many processing stages finally generating an export file for the client app
        /// </summary>
        public async Task ProcessFile()
        {
            var topics = _topicsLoaderService.TryLoadTopics();
            _gmSharedPins = _databaseLoaderService.LoadGMSharedPins();
            if (topics != null)
            {
                Topics = topics;
            }

            //_mapPinCache.Clean();
            ListPlaceNames();
            CheckSharedPinLocations();

            _aiVenueCleanUpService.ResetIsExportable(Topics);
            _aiVenueLocationService.RemoveDuplicatedPins(Topics);
            _pinCacheSyncService.MakeSureIndexIsInSearchString();
            _pinCacheSyncService.CheckRestaurantTypes();
            //_pinCacheSyncService.CheckPriceExtraction();
            //_pinCacheSyncService.CheckFilteredRestaurantTypes();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nReading captured FB data");
            _topicLoaderService.ReadFileLineByLine(_responsefileName, Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing information from source");
            _topicEmbeddedLinkService.UpdateMessageAndResponseUrls(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating country names from topic title");
            await _topicMetaCreationService.CategoriseTopic(Topics);
            await _topicMetaCreationService.CalculateLanguageTopic(Topics);
            await _topicMetaCreationService.TranslateTopic(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nStarting AI processing - detecting venue name/address");
            await _aiVenueCreationService.ScanTopicsUseAiToDetectVenueInfo(Topics);
            await _topicMetaCreationService.ScanTopicsDetectCountryAndCity(Topics);
            //await ScanTopicsRegenerateNullPins();
            //FixCity(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nFiltering AI pins");
            _aiVenueCleanUpService.RemoveGenericPlaceNames(Topics);
            //_aiVenueCleanUpService.CheckForPlaceNamesPinMisMatch(Topics);

            //_aiVenueCleanUpService.RemoveCityPins(Topics);
            _aiVenueCleanUpService.DiscoverMinMessageLength(Topics);
            _aiVenueLocationService.RemoveDuplicatedPins(Topics);
            _aiVenueCleanUpService.RemoveNullAiPins(Topics);
            _aiVenueCleanUpService.TagAiPinsFoundInDifferentCountry(Topics);
            _aiVenueCleanUpService.TagGenericPlaceNames(Topics);
            _aiVenueCleanUpService.TagAiPinsWithBadRestaurantTypes(Topics);

            _aiVenueCleanUpService.TagAiPinsPermanentlyClosed(Topics);
            _aiVenueCleanUpService.TagAiPinsNotFoundInOriginalText(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nRebuilding chain pins");
            //Enable if we have better filtering
            //_aiVenueCleanUpService.RemoveChainGeneratedAiPins(Topics);
            //_aiVenueLocationService.UpdateChainGeneratedPins(Topics);
            //_aiVenueLocationService.CheckNonExportable(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nUpdating pin information for Ai Venues");
            _aiVenueLocationService.ProcessNewPins(Topics);
            _aiVenueLocationService.CheckPinsAreInCache(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nExtracting meta info for pins");
            _pinCacheSyncService.ExtractMetaInfoFromPinCache();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing topics, generating short titles");
            await _topicMetaCreationService.GenerateShortTitles(Topics);

            Console.WriteLine("--------------------------------------");
            _aiVenueCleanUpService.CountPins(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating data for client application");
            await _clientExportFileGenerator.GenerateTopicExport(Topics);
            _topicsLoaderService.SaveTopics(Topics);


            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
        }

        private void ListPlaceNames()
        {
            var placeNames = _aiVenueCleanUpService.GetPlaceNames(Topics);
            var places = "";
            Console.WriteLine($"----------------------");
            foreach (var item in placeNames)
            {
                if (SmartPlaceNameFilterHelper.IsSkippable(item))
                {
                    Console.WriteLineRed($"Skipping :{item}");
                }

                places += $"{item}\r\n";
            }

            string filePath = "D:\\Coding\\Gluten\\Outputs\\";
            File.WriteAllText(filePath + "placeNames.txt", places);
        }

        private void CheckSharedPinLocations()
        {
            int searchesDone = 0;
            for (int i = 0; i < _gmSharedPins.Count; i++)
            {
                if (i < 1452) continue;
                var item = _gmSharedPins[i];
                item.Comment ??= item.Description;
                if (item.Label == null) continue;
                Console.WriteLine($"Processing {i} of {_gmSharedPins.Count} updating shared GM pins");
                if (searchesDone > 50)
                {
                    _databaseLoaderService.SaveGMSharedPins(_gmSharedPins);
                    _databaseLoaderService.SavePinDB();
                    searchesDone = 0;
                }
                if (string.IsNullOrWhiteSpace(item.GeoLatitude))
                {
                    var cachePin = _mapPinCache.TryGetPin(item.Label, "");
                    if (cachePin == null)
                    {
                        _mapPinService.GetMapUrl(item.Label);
                        cachePin = _mapPinService.GetPinFromCurrentUrl(item.Label, item.Label);

                        if (cachePin == null)
                        {
                            List<string> chainUrls = [];
                            _aiVenueProcessorService.IsPlaceNameAChain(chainUrls, item.Label);
                            if (chainUrls.Count == 1)
                            {
                                _mapPinService.GoToUrl(chainUrls[0]);
                                cachePin = _mapPinService.TryToGenerateMapPin(chainUrls[0], item.Label, "", item.Label);
                            }
                        }

                    }
                    if (cachePin != null)
                    {
                        if (cachePin.MetaData != null)
                        {
                            item.Price = cachePin.MetaData.Price;
                            item.RestaurantType = cachePin.MetaData.RestaurantType;
                            item.Stars = cachePin.MetaData.Stars;
                        }
                        item.MapsUrl = cachePin.MapsUrl;
                        item.GeoLatitude = cachePin.GeoLatitude;
                        item.GeoLongitude = cachePin.GeoLongitude;
                    }

                }
            }

            Console.WriteLine($"Completed shared pin update");
            _databaseLoaderService.SaveGMSharedPins(_gmSharedPins);
            _databaseLoaderService.SavePinDB();

        }

    }
}
