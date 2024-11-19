using AutoMapper;
using Azure.AI.TextAnalytics;
using Frodo.FacebookModel;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// </summary>
    internal class DataSyncService
    {
        private TopicHelper _topicHelper = new TopicHelper();
        private TopicsHelper _topicsHelper = new TopicsHelper();
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor;
        private AnalyzeDocumentService _analyzeDocumentService = new AnalyzeDocumentService();
        private TopicLoaderService _topicLoaderService = new TopicLoaderService();
        private PinHelper _pinHelper;
        private AIProcessingService _aIProcessingService;
        private DatabaseLoaderService _databaseLoaderService;

        public List<DetailedTopic> Topics = new List<DetailedTopic>();
        private string DBFileName = "D:\\Coding\\Gluten\\Topics.json";

        public DataSyncService(AIProcessingService aIProcessingService,
            SeleniumMapsUrlProcessor seleniumMapsUrlProcessor,
            PinHelper pinHelper,
            DatabaseLoaderService databaseLoaderService)
        {
            _aIProcessingService = aIProcessingService;
            _seleniumMapsUrlProcessor = seleniumMapsUrlProcessor;
            _pinHelper = pinHelper;
            _databaseLoaderService = databaseLoaderService;
        }


        public void ProcessFile()
        {
            var topics = _topicsHelper.TryLoadTopics(DBFileName);
            if (topics != null)
            {
                Topics = topics;
            }

            _topicLoaderService.ReadFileLineByLine("D:\\Coding\\Gluten\\Smeagol\\bin\\Debug\\net8.0\\Responses.txt", Topics);

            _analyzeDocumentService.OpenAgent();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing information from source");
            UpdateMessageAndResponseUrls();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nStarting AI processing - detecting venue name/address");
            //TODO: Skip for now - ScanTopicsUseAiToDetectVenueInfo();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nUpdating pin information for Ai Venues");
            UpdatePinsForAiVenues();
            _topicsHelper.SaveTopics(DBFileName, Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating data for client application");
            GenerateTopicExport();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
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
                    _topicsHelper.SaveTopics(DBFileName, Topics);
                }
            }
        }

        /// <summary>
        /// Group data by pin (venue), export to json
        /// </summary>
        private void GenerateTopicExport()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DetailedTopic, Topic>();
                cfg.CreateMap<DetailedTopic, PinLinkInfo>();
                cfg.CreateMap<Topic, PinLinkInfo>();
            });

            var mapper = config.CreateMapper();
            List<Topic> tempTopics = new List<Topic>();
            foreach (var t in Topics)
            {
                var newT = mapper.Map<Topic>(t);
                tempTopics.Add(newT);
            }
            List<PinTopic>? pins = _databaseLoaderService.LoadPinTopics();
            if (pins == null) pins = new List<PinTopic>();
            pins = DataHelper.ExtractPinExport(pins, tempTopics, mapper);

            var ii = 0;
            foreach (var pin in pins)
            {
                if (string.IsNullOrEmpty(pin.Description))
                {
                    var message = "";
                    if (pin.Topics != null)
                    {
                        foreach (var item in pin.Topics)
                        {
                            message += " " + item.Title;
                        }
                        pin.Description = _analyzeDocumentService.ExtractDescriptionTitle(message, pin.Label);
                        _databaseLoaderService.SavePinTopics(pins);
                    }
                }
                Console.WriteLine($"Updating descriptions - {ii}");
                ii++;
            }

            _databaseLoaderService.SavePinTopics(pins);
        }


        /// <summary>
        /// Try to sync Topic data with data extracted from the web
        /// </summary>
        private void CheckForUpdatedUrls(DetailedTopic topic, List<TopicLink> newUrls)
        {
            if (topic.UrlsV2 == null) return;
            if (topic.UrlsV2.Count != newUrls.Count)
            {
                Console.WriteLine("Mismatch in url detection");
            }
            else
            {
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    if (topic.UrlsV2[t].Pin == null)
                    {
                        if (topic.UrlsV2[t].Url != newUrls[t].Url
                            && !topic.UrlsV2[t].Url.Contains("/@")
                            && !topic.UrlsV2[t].Url.Contains("https://www.google.com/maps/d/viewer"))
                        {
                            topic.UrlsV2[t].Url = newUrls[t].Url;
                        }
                    }
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
            string unProcessedUrls = "";
            int unProcessedUrlsCount = 0;
            int pinCount = 0;
            bool tryProcessLinks = true;
            for (int i = 0; i < Topics.Count; i++)
            {
                Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                string responseText = "";
                foreach (var response in topic.ResponsesV2)
                {
                    responseText += response + " , ";
                }
                // Update url list from title
                topic.HashTags = StringHelper.ExtractHashtags(topic.Title);
                var newUrls = StringHelper.ExtractUrls(topic.Title);
                if (topic.UrlsV2 == null)
                {
                    topic.UrlsV2 = newUrls;
                }
                else
                {
                    CheckForUpdatedUrls(topic, newUrls);
                }

                // Check for links in topics
                if (topic.HasLink()) linkCount++;
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    var url = topic.UrlsV2[t].Url;

                    if (topic.UrlsV2[t].Pin == null
                        && tryProcessLinks)
                    {
                        if (topic.UrlsV2[t].Pin == null)
                        {
                            var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(url);
                            topic.UrlsV2[t].Url = newUrl;
                            topic.UrlsV2[t].Pin = _pinHelper.TryToGenerateMapPin(newUrl, false);
                            mapsCallCount++;
                        }

                    }
                    else
                    {
                        //TODO: Not sure about data structure?
                        //topic.UrlsV2[t].Pin.MapsUrl = topic.UrlsV2[t].Url;
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

                            if (links[t].Pin == null
                                && tryProcessLinks)
                            {
                                if (links[t].Pin == null)
                                {
                                    var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(url);
                                    links[t].Url = newUrl;
                                    links[t].Pin = _pinHelper.TryToGenerateMapPin(newUrl, false);
                                    mapsCallCount++;
                                }

                            }
                            mapsLinkCount++;
                        }
                    }
                }

                if (topic.HasMapPin() && topic.HasMapLink())
                {
                    unProcessedUrlsCount++;
                    unProcessedUrls += topic.UrlsV2[0] + "/r/n";
                }

                if (topic.ResponseHasLink) responseLinkCount++;
                if (topic.ResponseHasMapLink) responseMapsLinkCount++;

                pinCount += topic.UrlsV2.Where(o => o.Pin != null).Count();

                foreach (var response in topic.ResponsesV2)
                {
                    if (response.Links != null)
                    {
                        pinCount += response.Links.Where(o => o.Pin != null).Count();
                    }
                }
            }

            Console.WriteLine($"Pin Count : {pinCount}");
            Console.WriteLine($"Unprocessed Links : {unProcessedUrlsCount}");
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
            var aiPin = new AiPinGeneration(_aIProcessingService);
            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                Console.WriteLine($"Processing AI pins {i} of {Topics.Count}");
                if (i % 10 == 0)
                {
                    _topicsHelper.SaveTopics(DBFileName, Topics);
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
                        ai.Pin = cachedPin;
                        continue;
                    }

                    if (ai.Pin != null)
                    {
                        cachedPin = _pinHelper.TryGetPin(ai.Pin.Label);
                        if (cachedPin != null)
                        {
                            ai.Pin = cachedPin;
                            continue;
                        }
                        _databaseLoaderService.SavePinDB();
                    }

                    if (ai.Pin == null)
                    {
                        var groupId = "379994195544478";
                        if (string.IsNullOrWhiteSpace(topic.GroupId))
                        {
                            Console.WriteLine($"Unknown group Id {i}, using default");
                            //continue;
                        }
                        else
                        {
                            groupId = topic.GroupId;
                        }

                        aiPin.GetMapPinForPlaceName(ai, groupId);

                        // If we are unable to get a specific pin, generate chain urls, to add later
                        if (ai.Pin == null)
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

                // Remove any pins that dont have names, and hotels
                for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
                {
                    if (string.IsNullOrWhiteSpace(topic.AiVenues[t].PlaceName)
                        || topic.AiVenues[t].PlaceName.ToLower().Contains("Hotel"))
                    {
                        topic.AiVenues.RemoveAt(t);
                    }
                }

                // Add any chain urls detected earlier (searches that have multiple results)
                foreach (var item in chainUrls)
                {
                    var pin = _aIProcessingService.GetPinFromCurrentUrl(item, true);
                    if (pin != null)
                    {
                        // Add pin to AiGenerated
                        var newVenue = new AiVenue();
                        newVenue.Pin = pin;
                        newVenue.PlaceName = pin.Label;
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
