using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System;

namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// All the magic happens here
    /// </summary>
    internal class DataSyncService(MapPinService _mapPinService,
        DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        ClientExportFileGenerator _clientExportFileGenerator,
        GeoService _geoService,
        FBGroupService _fBGroupService,
        MapPinCache _mapPinCache,
        PinCacheSyncService _pinCacheSyncService,
        CityService _cityService,
        AiVenueLocationService _aiVenueLocationService,
        TopicsDataLoaderService _topicsHelper,
        AiVenueCleanUpService _aiVenueCleanUpService,
        IConsole Console
        )
    {
        private readonly string _responsefileName = "D:\\Coding\\Gluten\\Database\\Responses.txt";
        private readonly LocalAiInterfaceService _localAi = new(Console);
        private readonly TopicLoaderService _topicLoaderService = new(Console);

        public List<DetailedTopic> Topics = [];
        private readonly bool _regeneratePins = false;
        private List<AiVenue> _placeNameSkipList = [];
        private readonly int _lastImportedIndex = 0;

        /// <summary>
        /// Processes the file generated from FB, run through many processing stages finally generating an export file for the client app
        /// </summary>
        public async Task ProcessFile()
        {
            var skipSomeOperationsForDebugging = false;
            var topics = _topicsHelper.TryLoadTopics();
            if (topics != null)
            {
                Topics = topics;
            }

            //await _localAi.GenerateShortTitle("this is a test message,this is a test message,this is a test message,this is a test message,this is a test message,this is a test message,this is a test message,this is a test message");

            _placeNameSkipList = _databaseLoaderService.LoadPlaceSkipList();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nReading captured FB data");
            _topicLoaderService.ReadFileLineByLine(_responsefileName, Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating country names from topic title");
            await ScanTopicsUseAiToDetectTopicCountry();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing topics, generating short titles");
            await GenerateShortTitles();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing information from source");
            UpdateMessageAndResponseUrls();


            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nFiltering AI pins");
            _aiVenueCleanUpService.RemoveAiPinsInBadLocations(Topics);
            _aiVenueCleanUpService.RemoveAiPinsWithBadRestaurantTypes(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nStarting AI processing - detecting venue name/address");
            if (!skipSomeOperationsForDebugging)
                await ScanTopicsUseAiToDetectVenueInfo();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nUpdating pin information for Ai Venues");
            _aiVenueLocationService.UpdatePinsForAiVenues(Topics, _regeneratePins, _placeNameSkipList);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nExtracting meta info for pins");
            _pinCacheSyncService.ExtractMetaInfoFromPinCache();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nFiltering AI pins");
            _aiVenueCleanUpService.RemoveAiPinsInBadLocations(Topics);


            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating data for client application");
            await _clientExportFileGenerator.GenerateTopicExport(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
        }

        private async Task GenerateShortTitles()
        {
            int shortTitlesAdded = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                if (i % 10 == 0)
                    Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                if (string.IsNullOrWhiteSpace(topic.ShortTitle) && !topic.ShortTitleProcessed)
                {
                    topic.ShortTitle = await _localAi.GenerateShortTitle(topic.Title);
                    topic.ShortTitleProcessed = true;
                    if (!string.IsNullOrWhiteSpace(topic.ShortTitle))
                    {
                        shortTitlesAdded++;
                    }
                }
            }

            _topicsHelper.SaveTopics(Topics);
            Console.WriteLine($"Added short title : {shortTitlesAdded}");
        }



        /// <summary>
        /// Use AI to extract information from unformatted human generated data
        /// </summary>
        private async Task ScanTopicsUseAiToDetectVenueInfo()
        {
            for (int i = 0; i < Topics.Count; i++)
            {
                if ((Topics[i].AiVenues == null && !Topics[i].AiIsQuestion))
                {
                    Console.WriteLine($"Processing topic message {i} of {Topics.Count}");
                    if (i % 100 == 0)
                    {
                        _topicsHelper.SaveTopics(Topics);
                    }

                    DetailedTopic? topic = Topics[i];
                    var venue = await _localAi.ExtractRestaurantNamesFromTitle(topic.Title, topic);
                    topic.AiVenues = venue;
                }
            }
            _topicsHelper.SaveTopics(Topics);
        }

        private async Task ScanTopicsUseAiToDetectTopicCountry()
        {
            var countries = _geoService.GetCountries();

            for (int i = 0; i < Topics.Count; i++)
            {
                if (i < 31129) continue;
                if (!string.IsNullOrWhiteSpace(Topics[i].TitleCountry)) continue;

                Console.WriteLine($"Extracting city/country names {i} of {Topics.Count}");
                DetailedTopic? topic = Topics[i];

                if (string.IsNullOrWhiteSpace(topic.TitleCity))
                {
                    var city = await _localAi.ExtractCity(topic.Title);
                    if (topic.Title.Contains(city))
                    {
                        if (_cityService.IsCity(city))
                        {
                            topic.TitleCity = city;
                        }
                        else
                        {
                            Console.WriteLine($"Unknown city {city}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"City not found in message");
                    }
                }

                var country = await _localAi.ExtractCountry(topic.Title) ?? "";
                country = country.Replace("United States of America", "United States");
                country = country.Replace("UK", "United Kingdom");
                country = country.Replace("The Netherlands", "Netherlands");
                country = country.Replace("Nederland", "Netherlands");
                country = country.Replace("SPAIN", "Spain");
                country = country.Replace("\"", "");
                country = country.Replace("Suid Afrika", "South Africa");
                country = country.Replace("Türkiye", "Turkey");


                if (string.IsNullOrWhiteSpace(country)) continue;
                if (country.Contains("\r\n")) continue;
                if (country == "Europe") continue;

                if (!countries.Exists(o => o == country)
                    && !countries.Exists(o => o == country.Replace("é", "e"))
                    && country != "Peru")
                {
                    Console.WriteLineRed($"Unknown country {country}");
                    continue;
                }
                topic.TitleCountry = country.Replace("é", "e");
            }
            _topicsHelper.SaveTopics(Topics);
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
                if (i < _lastImportedIndex) continue;
                Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                if (string.IsNullOrWhiteSpace(topic.GroupId))
                {
                    Console.WriteLine($"Missing group id : {i}, {topic.Title} ");
                    continue;
                }
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                // Update url list from title
                var newUrls = TopicExtractionHelper.ExtractUrls(topic.Title);
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
                        var newUrl = _mapPinService.CheckUrlForMapLinks(url);
                        topic.UrlsV2[t].Url = newUrl;
                        var cachePin = _mapPinService.TryToGenerateMapPin(newUrl, url, groupCountry);
                        if (cachePin != null)
                        {
                            if (string.IsNullOrWhiteSpace(cachePin.MetaHtml))
                            {
                                cachePin.MetaHtml = _mapPinService.GetMeta(cachePin.Label);
                            }
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

                    var newLinks = TopicExtractionHelper.ExtractUrls(message);
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
                                var newUrl = _mapPinService.CheckUrlForMapLinks(url);
                                links[t].Url = newUrl;
                                var cachePin = _mapPinService.TryToGenerateMapPin(newUrl, url, groupCountry);
                                if (cachePin != null)
                                {
                                    if (string.IsNullOrWhiteSpace(cachePin.MetaHtml))
                                    {
                                        cachePin.MetaHtml = _mapPinService.GetMeta(cachePin.Label);
                                    }
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

            _topicsHelper.SaveTopics(Topics);
            Console.WriteLine($"Has Links : {linkCount}");
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
            Console.WriteLine($"Has Response Links : {responseLinkCount}");
            Console.WriteLine($"Has Response Maps Links : {responseMapsLinkCount}");
        }



    }
}
