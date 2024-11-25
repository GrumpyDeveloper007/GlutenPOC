using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data;
using Gluten.Data.ClientModel;
using Gluten.Data.PinCache;
using Gluten.Data.PinDescription;
using Gluten.Data.TopicModel;

namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// All the magic happens here
    /// </summary>
    internal class DataSyncService(MapPinService _aIProcessingService,
        SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor,
        PinHelper _pinHelper,
        DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        ClientExportFileGenerator _clientExportFileGenerator)
    {
        private readonly TopicsHelper _topicsHelper = new();
        private readonly LocalAiInterfaceService _analyzeDocumentService = new();
        private readonly TopicLoaderService _topicLoaderService = new();
        private readonly MapsMetaExtractorService _mapsMetaExtractorService = new();
        private readonly FBGroupService _fbGroupService = new();

        public List<DetailedTopic> Topics = [];
        private readonly bool _regeneratePins = false;

        /// <summary>
        /// Processes the file generated from FB, run through many processing stages finally generating an export file for the client app
        /// </summary>
        public void ProcessFile()
        {
            var topics = _topicsHelper.TryLoadTopics();
            if (topics != null)
            {
                Topics = topics;
            }

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nReading captured FB data");
            _topicLoaderService.ReadFileLineByLine("D:\\Coding\\Gluten\\Smeagol\\bin\\Debug\\net8.0\\Responses.txt", Topics);

            //FixData();
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing topics, generating short titles");
            GenerateShortTitles();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing information from source");
            UpdateMessageAndResponseUrls();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nStarting AI processing - detecting venue name/address");
            ScanTopicsUseAiToDetectVenueInfo();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nUpdating pin information for Ai Venues");
            UpdatePinsForAiVenues();
            _topicsHelper.SaveTopics(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nExtracting meta info for pins");
            ExtractMetaInfoFromPinCache();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating data for client application");
            _clientExportFileGenerator.GenerateTopicExport(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
        }

        private void GenerateShortTitles()
        {
            for (int i = 0; i < Topics.Count; i++)
            {
                Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                if (string.IsNullOrWhiteSpace(topic.ShortTitle))
                {
                    topic.ShortTitle = _analyzeDocumentService.GenerateShortTitle(topic.Title);
                }
            }
        }

        private void ExtractMetaInfoFromPinCache()
        {
            int i = 0;
            var cache = _pinHelper.GetCache();
            _mapsMetaExtractorService.ClearRestaurantType();
            foreach (var item in cache)
            {
                Console.WriteLine($"Processing pin meta {i} or {cache.Count}");
                if (string.IsNullOrWhiteSpace(item.Value.MetaHtml) && item.Value.MapsUrl != null)
                {
                    // load meta if missing
                    _seleniumMapsUrlProcessor.GoAndWaitForUrlChange(item.Value.MapsUrl);
                    item.Value.MetaHtml = _seleniumMapsUrlProcessor.GetMeta(item.Value.Label);
                }

                if (item.Value.MetaData == null || string.IsNullOrWhiteSpace(item.Value.MetaData.RestaurantType))
                {
                    item.Value.MetaData = _mapsMetaExtractorService.ExtractMeta(item.Value.MetaHtml);
                }
                if (item.Value.MetaData != null)
                {
                    _mapsMetaExtractorService.AddRestaurantType(item.Value.MetaData.RestaurantType);
                }
                else
                {
                    Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                }
                i++;
            }
            var restaurants = _mapsMetaExtractorService.GetRestaurantTypes();
            _databaseLoaderService.SaveRestaurantList(restaurants);
            _databaseLoaderService.SavePinDB();
        }


        /// <summary>
        /// Use AI to extract information from unformatted human generated data
        /// </summary>
        private void ScanTopicsUseAiToDetectVenueInfo()
        {
            for (int i = 0; i < Topics.Count; i++)
            {
                if (Topics[i].AiVenues == null && !Topics[i].AiIsQuestion)
                {
                    Console.WriteLine($"Processing topic message {i} of {Topics.Count}");
                    DetailedTopic? topic = Topics[i];
                    var venue = _analyzeDocumentService.ExtractRestaurantNamesFromTitle(topic.Title, ref topic);
                    topic.AiVenues = venue;
                    _topicsHelper.SaveTopics(Topics);
                }
            }
        }

        /// <summary>
        /// Scan detected urls, try to generate pin information
        /// </summary>
        private void UpdateMessageAndResponseUrls()
        {
            int linkCount = 0;
            int mapsLinkCount = 0;
            int responseLinkCount = 0;
            int responseMapsLinkCount = 0;
            int mapsCallCount = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                // Update url list from title
                topic.HashTags = StringHelper.ExtractHashtags(topic.Title);
                var newUrls = StringHelper.ExtractUrls(topic.Title);
                if (topic.UrlsV2 == null)
                {
                    topic.UrlsV2 = newUrls;
                }
                else
                {
                    DataHelper.CheckForUpdatedUrls(topic, newUrls);
                }

                // Check for links in topics
                if (topic.HasLink()) linkCount++;
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    var url = topic.UrlsV2[t].Url;

                    if (topic.UrlsV2[t].Pin == null || _regeneratePins)
                    {
                        var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(url);
                        var meta = _seleniumMapsUrlProcessor.GetMeta(null);
                        topic.UrlsV2[t].Url = newUrl;
                        var cachePin = _pinHelper.TryToGenerateMapPin(newUrl, false, url, meta);
                        if (cachePin != null)
                        {
                            var newPin = _mappingService.Map<TopicPin, TopicPinCache>(cachePin);
                            topic.UrlsV2[t].Pin = newPin;
                        }
                        mapsCallCount++;
                    }

                    mapsLinkCount++;
                }

                // Look for links in the responses
                for (int z = 0; z < topic.ResponsesV2.Count; z++)
                {
                    var message = topic.ResponsesV2[z].Message;
                    if (message == null) continue;

                    var newLinks = StringHelper.ExtractUrls(message);
                    if (topic.ResponsesV2[z].Links == null)
                    {
                        topic.ResponsesV2[z].Links = newLinks;
                    }

                    var links = topic.ResponsesV2[z].Links;
                    if (links != null)
                    {
                        for (int t = 0; t < links.Count; t++)
                        {
                            var url = links[t].Url;

                            if (links[t].Pin == null || _regeneratePins)
                            {
                                var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(url);
                                var meta = _seleniumMapsUrlProcessor.GetMeta(null);
                                links[t].Url = newUrl;
                                var cachePin = _pinHelper.TryToGenerateMapPin(newUrl, false, url, meta);
                                if (cachePin != null)
                                {
                                    var newPin = _mappingService.Map<TopicPin, TopicPinCache>(cachePin);
                                    links[t].Pin = newPin;
                                }
                                mapsCallCount++;
                            }

                            mapsLinkCount++;
                        }
                    }
                }

                if (topic.ResponseHasLink) responseLinkCount++;
                if (topic.ResponseHasMapLink) responseMapsLinkCount++;
            }

            Console.WriteLine($"Has Links : {linkCount}");
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
            Console.WriteLine($"Has Response Links : {responseLinkCount}");
            Console.WriteLine($"Has Response Maps Links : {responseMapsLinkCount}");
        }

        /// <summary>
        /// Scan generated Ai Venues and try to generate any missing pins
        /// </summary>
        private void UpdatePinsForAiVenues()
        {
            var aiPin = new AiVenueProcessorService(_aIProcessingService, _mappingService, _fbGroupService);
            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                Console.WriteLine($"Processing AI pins {i} of {Topics.Count}");
                if (i % 10 == 0)
                {
                    _topicsHelper.SaveTopics(Topics);
                    _databaseLoaderService.SavePinDB();
                }

                // Remove any duplicated pins
                if (topic.AiVenues != null)
                {
                    for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
                    {
                        if (DataHelper.IsInList(topic.AiVenues, topic.AiVenues[t], t))
                        {
                            topic.AiVenues.RemoveAt(t);
                        }
                    }
                }

                if (topic.AiVenues == null) continue;
                var chainUrls = new List<string>();
                for (int t = 0; t < topic.AiVenues.Count; t++)
                {
                    AiVenue? ai = topic.AiVenues[t];
                    // Only process unprocessed pins

                    var cachedPin = _pinHelper.TryGetPin(ai.PlaceName);
                    if (cachedPin != null)
                    {
                        ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
                        continue;
                    }

                    // alternate search
                    if (ai.Pin != null)
                    {
                        cachedPin = _pinHelper.TryGetPin(ai.Pin.Label);
                        if (cachedPin != null)
                        {
                            ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
                            continue;
                        }
                    }

                    if (ai.Pin == null)//|| _regeneratePins
                    {
                        var groupId = FBGroupService.DefaultGroupId;
                        if (string.IsNullOrWhiteSpace(topic.GroupId))
                        {
                            Console.WriteLine($"Unknown group Id {i}, using default");
                            //continue;
                        }
                        else
                        {
                            groupId = topic.GroupId;
                        }

                        if (ai.Pin == null || _regeneratePins)
                        {
                            aiPin.GetMapPinForPlaceName(ai, groupId);
                        }

                        // If we are unable to get a specific pin, generate chain urls, to add later
                        if (ai.Pin == null)//|| (_regeneratePins && currentUrl != null)
                        {
                            Console.WriteLine($"Searching for a chain");
                            if (aiPin.IsPlaceNameAChain(ai, chainUrls, groupId))
                            {
                                ai.PlaceName = null;
                                ai.Address = null;
                            }
                        }
                    }
                }

                // Remove any pins that dont have names
                for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
                {
                    if (string.IsNullOrWhiteSpace(topic.AiVenues[t].PlaceName))
                    {
                        topic.AiVenues.RemoveAt(t);
                    }
                }

                // Add any chain urls detected earlier (searches that have multiple results)
                foreach (var item in chainUrls)
                {
                    var pin = _aIProcessingService.GetPinFromCurrentUrl(item, true, "");
                    if (pin != null)
                    {
                        // Add pin to AiGenerated
                        var newVenue = new AiVenue
                        {
                            Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin),
                            PlaceName = pin.Label
                        };
                        if (!DataHelper.IsInList(topic.AiVenues, newVenue, -1))
                        {
                            topic.AiVenues.Add(newVenue);
                            Console.WriteLine($"Adding chain url {pin.Label}");
                        }
                    }
                }

            }

        }

    }
}
