using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System;
using System.Diagnostics;
using System.Globalization;

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
        TopicsDataLoaderService _topicsLoaderService,
        AiVenueCleanUpService _aiVenueCleanUpService,
        AiInterfaceService _localAi,
        IConsole Console
        )
    {
        private readonly string _responsefileName = "D:\\Coding\\Gluten\\Database\\Responses.txt";
        private readonly TopicLoaderService _topicLoaderService = new(_topicsLoaderService, Console);

        public List<DetailedTopic> Topics = [];
        private readonly bool _regeneratePins = true;

        /// <summary>
        /// Processes the file generated from FB, run through many processing stages finally generating an export file for the client app
        /// </summary>
        public async Task ProcessFile()
        {
            var topics = _topicsLoaderService.TryLoadTopics();
            if (topics != null)
            {
                Topics = topics;
            }
            _mapPinCache.Clean();

            var venues = _databaseLoaderService.LoadPlaceSkipList();
            string temp = "";
            venues.Sort((x, y) => y.PlaceName.CompareTo(x.PlaceName));
            foreach (var venue in venues)
            {
                if (venue == null) continue;
                temp += $"\"{venue.PlaceName}\",\n";
            }

            _aiVenueCleanUpService.ResetIsExportable(Topics);
            var placeNames = _aiVenueCleanUpService.GetPlaceNames(Topics);
            string filePath = "D:\\Coding\\Gluten\\Outputs\\";
            File.WriteAllText(filePath + "placeNames.txt", placeNames);


            _pinCacheSyncService.MakeSureIndexIsInSearchString();
            _pinCacheSyncService.CheckRestaurantTypes();
            _pinCacheSyncService.CheckPriceExtraction();


            //await _localAi.ExtractDescriptionTitle("this is a test message,this is a test message,this is a test message,this is a test message,this is a test message,this is a test message,this is a test message,this is a test message", "test");

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nReading captured FB data");
            _topicLoaderService.ReadFileLineByLine(_responsefileName, Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing information from source");
            UpdateMessageAndResponseUrls();

            await CategoriseTopic();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nStarting AI processing - detecting venue name/address");
            await ScanTopicsUseAiToDetectVenueInfo();
            //await ScanTopicsRegenerateNullPins();
            //FixCity(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating country names from topic title");
            await ScanTopicsDetectCountryAndCity();

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
            _aiVenueCleanUpService.CountPins(Topics);


            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nUpdating pin information for Ai Venues");
            _aiVenueLocationService.ProcessNewPins(Topics);
            _aiVenueLocationService.CheckPinsAreInCache(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nExtracting meta info for pins");
            _pinCacheSyncService.ExtractMetaInfoFromPinCache();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nProcessing topics, generating short titles");
            await GenerateShortTitles();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating data for client application");
            await _clientExportFileGenerator.GenerateTopicExport(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
        }

        private async Task GenerateShortTitles()
        {
            int shortTitlesAdded = 0;
            int itemsUpdated = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                var topic = Topics[i];
                if (itemsUpdated > 50)
                {
                    Console.WriteLineBlue("Saving topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }

                if (topic.ShortTitleProcessed) continue;
                if (i % 10 == 0)
                    Console.WriteLine($"Generating short title {i} of {Topics.Count}");

                if ((topic.AiVenues == null || topic.AiVenues.Count == 0)
                    && (topic.UrlsV2 == null || topic.UrlsV2.Count == 0)
                    && (topic.ResponsesV2 == null || topic.ResponsesV2.Count == 0)) continue;

                if (string.IsNullOrWhiteSpace(topic.ShortTitle))//&& !topic.ShortTitleProcessed
                {
                    topic.ShortTitle = await _localAi.GenerateShortTitle(topic.Title);
                    topic.ShortTitleProcessed = true;
                    if (!string.IsNullOrWhiteSpace(topic.ShortTitle))
                    {
                        shortTitlesAdded++;
                        itemsUpdated++;
                    }
                }
            }

            _topicsLoaderService.SaveTopics(Topics);
            Console.WriteLine($"Added short title : {shortTitlesAdded}");
        }



        /// <summary>
        /// Use AI to extract information from unformatted human generated data
        /// </summary>
        private async Task ScanTopicsUseAiToDetectVenueInfo()
        {
            bool foundUpdates = false;
            Stopwatch timer = new();
            for (int i = 0; i < Topics.Count; i++)
            {
                if (i < 30520) continue;

                DetailedTopic? topic = Topics[i];
                //if (topic.IsAiVenuesSearchDone) continue;
                if (_aiVenueLocationService.IsRecipe(topic)) continue;
                if (_aiVenueLocationService.IsTopicAQuestion(topic)) continue;

                if (i % 100 == 0 && foundUpdates)
                {
                    Console.WriteLineBlue($"Saving topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    foundUpdates = false;
                }
                timer.Restart();
                var venue = await _localAi.ExtractRestaurantNamesFromTitle(topic.Title);
                timer.Stop();
                Console.WriteLine($"Processing topic (new) {i} of {Topics.Count} length :{topic.Title.Length} Time: {timer.Elapsed.TotalSeconds}");
                topic.IsAiVenuesSearchDone = true;
                if (SyncVenues(topic.AiVenues, venue))
                {
                    foundUpdates = true;
                }
            }
            _topicsLoaderService.SaveTopics(Topics);
        }

        private void FixCity(List<DetailedTopic> topics)
        {
            for (int i = 0; i < topics.Count; i++)
            {
                DetailedTopic? topic = topics[i];
                if (topic.AiVenues == null) continue;

                for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
                {
                    AiVenue? ai = topic.AiVenues[t];
                    if (!string.IsNullOrWhiteSpace(ai.City) && !_cityService.IsCity(ai.City))
                    {
                        ai.City = "";
                    }
                }
            }
            _topicsLoaderService.SaveTopics(Topics);
        }



        private async Task ScanTopicsRegenerateNullPins()
        {
            int updateCount = 0;
            Stopwatch timer = new();
            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                if (_aiVenueLocationService.IsRecipe(topic)) continue;
                if (topic.AiVenues == null) continue;
                if (_aiVenueLocationService.IsTopicAQuestion(topic)) continue;

                var reCheck = false;
                var allNull = true;
                if (topic.AiVenues != null)
                    foreach (var item in topic.AiVenues)
                    {
                        if (item.Pin != null) allNull = false;

                        if (item.Pin == null
                            && !item.IsChain)
                        {
                            reCheck = true;
                        }
                    }
                if (!reCheck) continue;

                if (updateCount > 50)
                {
                    Console.WriteLineBlue($"Saving topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    updateCount = 0;
                }
                timer.Restart();
                var venue = await _localAi.ExtractRestaurantNamesFromTitle(topic.Title);
                timer.Stop();
                Console.WriteLine($"Processing topic (null pin) {i} of {Topics.Count} length :{topic.Title.Length} Time: {timer.Elapsed.TotalSeconds}");
                topic.IsAiVenuesSearchDone = true;

                if (allNull)
                {
                    topic.AiVenues = venue;
                    updateCount++;
                }
                else
                {
                    if (SyncVenues(topic.AiVenues, venue))
                    {
                        updateCount++;
                    }
                }
            }
            _topicsLoaderService.SaveTopics(Topics);
        }

        private bool SyncVenues(List<AiVenue>? oldVenues, List<AiVenue>? newVenues)
        {
            bool foundUpdates = false;
            if (oldVenues == null && newVenues == null) return false;
            if (newVenues == null) return false;
            if (oldVenues == null && newVenues != null)
            {
                oldVenues = newVenues;
                return false;
            }

            for (int i = 0; i < newVenues.Count; i++)
            {
                var oldVenue = oldVenues.FirstOrDefault(o => o.PlaceName == newVenues[i].PlaceName);
                if (oldVenue != null)
                {
                    if (string.IsNullOrWhiteSpace(oldVenue.City)) oldVenue.City = newVenues[i].City;
                    foundUpdates = true;
                }
                else
                {
                    oldVenues.Add(newVenues[i]);
                    Console.WriteLineBlue($"Adding new venue :{newVenues[i].PlaceName}");
                    foundUpdates = true;
                }
            }
            return foundUpdates;
        }

        private async Task ScanTopicsDetectCountryAndCity()
        {
            var countries = _geoService.GetCountries();
            var unknownCities = new List<string>();
            var itemsUpdated = 0;

            for (int i = 0; i < Topics.Count; i++)
            {
                if (i < 51810) continue;
                DetailedTopic? topic = Topics[i];

                if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;
                if (_aiVenueLocationService.IsRecipe(topic)) continue;
                if (_aiVenueLocationService.IsTopicAQuestion(topic)) continue;
                if (!string.IsNullOrWhiteSpace(Topics[i].TitleCountry) && !string.IsNullOrWhiteSpace(Topics[i].TitleCity)) continue;
                //if (topic.TitleCategory != "DESCRIBE") continue;
                //if (topic.CitySearchDone) continue;
                if (itemsUpdated > 100)
                {
                    Console.WriteLineBlue("Saving Topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }

                Console.WriteLine($"Extracting city/country names {i} of {Topics.Count}");

                if (string.IsNullOrWhiteSpace(topic.TitleCity))
                {
                    var city = await _localAi.ExtractCity(topic.Title);
                    city = city.Replace("\r", "");
                    city = city.Replace("\n", "").Trim();

                    topic.CitySearchDone = true;
                    if (topic.Title.Contains(city))
                    {
                        if (_cityService.IsCity(city))
                        {
                            topic.TitleCity = city;
                        }
                        else
                        {
                            Console.WriteLine($"Unknown city {city}");
                            if (!unknownCities.Exists(o => o == city))
                                unknownCities.Add(city);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"City not found in message");
                    }
                }

                // todo: remove me
                if (!_fBGroupService.IsGenericGroup(topic.GroupId)) continue;

                if (string.IsNullOrWhiteSpace(topic.TitleCountry))
                {
                    var country = await _localAi.ExtractCountry(topic.Title) ?? "";
                    country = country.Replace("\r", "");
                    country = country.Replace("\n", "").Trim();
                    country = country.Replace("\"", "");

                    country = CapitalizeEachWord(country);

                    country = country.Replace("United States of America", "United States");
                    country = country.Replace("USA", "United States");
                    country = country.Replace("UK", "United Kingdom");
                    country = country.Replace("Wales", "United Kingdom");
                    country = country.Replace("The Netherlands", "Netherlands");
                    country = country.Replace("Nederland", "Netherlands");
                    country = country.Replace("Suid Afrika", "South Africa");
                    country = country.Replace("Türkiye", "Turkey");
                    country = country.Replace("Saint Lucia", "St. Lucia");
                    country = country.Replace("Bahamas", "The Bahamas");
                    country = country.Replace("Turks and Caicos Islands", "Turks & Caicos Is.");
                    country = country.Replace("Grand Cayman", "Cayman Is.");
                    country = country.Replace("Saint Kitts and Nevis", "St.Kitts & Nevis");
                    country = country.Replace("Cayman Islands", "Cayman Is.");
                    country = country.Replace("Giappone", "Japan");
                    country = country.Replace("Japón", "Japan");
                    country = country.Replace("Hong Kong", "China");
                    country = country.Replace("VIETNAM", "Vietnam");
                    country = country.Replace("Viet Nam", "Vietnam");
                    country = country.Replace("ไทย", "Thailand");
                    country = country.Replace("ประเทศThailand", "Thailand");
                    country = country.Replace("Kingdom of Thailand", "Thailand");
                    country = country.Replace("Gambia", "The Gambia");
                    country = country.Replace("Antigua", "Antigua & Barbuda");
                    country = country.Replace("Bosnia and Herzegovina", "Bosnia & Herzegovina");
                    country = country.Replace("North Macedonia", "Macedonia");
                    country = country.Replace("Ελλάδα", "Greece");
                    country = country.Replace("Mallorca", "Spain");
                    country = country.Replace("Kosovo", "Albania");
                    country = country.Replace("Republic of Malta", "Malta");
                    country = country.Replace("Belgium --", "Belgium");
                    country = country.Replace("Canary islands", "Spain");
                    country = country.Replace("Gran Canaria", "Spain");
                    country = country.Replace("Perú", "Peru (Peruvian point of view)");


                    country = country.Replace("Bali", "Indonesia");
                    country = country.Replace("Czechia", "Czech Republic");
                    country = country.Replace("Peru", "Peru(Peruvian point of view)");
                    if (country.StartsWith("Korea"))
                        country = country.Replace("Korea", "South Korea");

                    if (string.IsNullOrWhiteSpace(country)) continue;

                    if (country == "Europe") continue;

                    if (!countries.Exists(o => o == country)
                        && !countries.Exists(o => o == country.Replace("é", "e"))
                        && country != "Asia"
                        && country != "Europe" && country != "European Union" && country != "European"
                        && country != "South America"
                        )
                    {

                        if (country != "Safe") Console.WriteLineRed($"Unknown country : {country}");
                    }
                    else
                    {
                        topic.TitleCountry = country.Replace("é", "e");
                    }
                }

                if (topic.TitleCity != null)
                {
                    var countryForCity = _cityService.CityToCountry(topic.TitleCity);
                    var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                    if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";
                    if (!string.IsNullOrWhiteSpace(countryForCity) && !countryForCity.Contains(groupCountry, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLineRed($"Group Country :{groupCountry}, city country :{countryForCity}");
                    }
                }
                itemsUpdated++;

            }

            Console.WriteLineBlue("Unknown cities");
            foreach (var city in unknownCities)
            {
                Console.WriteLineBlue(city);
            }


            _topicsLoaderService.SaveTopics(Topics);
        }


        private async Task CategoriseTopic()
        {
            var itemsUpdated = 0;

            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];

                if (!string.IsNullOrWhiteSpace(topic.TitleCategory)) continue;
                if (itemsUpdated > 100)
                {
                    Console.WriteLineBlue("Saving Topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }


                var category = await _localAi.CategoriseMessage(topic.Title);
                Console.WriteLine($"Categorise topics {i} of {Topics.Count} : {category}");
                if (category == "DESCRIBE" || category == "QUESTION" || category == "UNKNOWN")
                {
                    topic.TitleCategory = category;
                }
                else if (category.Contains("DESCRIBE"))
                {
                    topic.TitleCategory = "DESCRIBE";
                }
                else
                {
                    Console.WriteLineRed($"Failed to categorise :{category}");
                }

                itemsUpdated++;
            }

            _topicsLoaderService.SaveTopics(Topics);
        }


        /// <summary>
        /// Scan detected urls, try to generate pin information
        /// </summary>
        private void UpdateMessageAndResponseUrls()
        {
            int mapsLinkCount = 0;
            int mapsCallCount = 0;
            int searchesDone = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                if (i < 55663) continue;
                Console.WriteLine($"Processing {i} of {Topics.Count} updating embedded urls");
                if (searchesDone > 50)
                {
                    _databaseLoaderService.SavePinDB();
                    searchesDone = 0;
                }
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
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    var url = topic.UrlsV2[t].Url;
                    if (!MapPinHelper.IsMapsUrl(url)) continue;

                    if (topic.UrlsV2[t].Pin == null || _regeneratePins)
                    {
                        var cachePin = _mapPinService.TryToGenerateMapPin(url, url, groupCountry, "");
                        if (cachePin == null)
                        {
                            searchesDone++;
                            var newUrl = _mapPinService.CheckUrlForMapLinks(url);
                            cachePin = _mapPinService.TryToGenerateMapPin(newUrl, url, groupCountry, "");
                        }
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
                            if (!MapPinHelper.IsMapsUrl(url)) continue;


                            if (links[t].Pin == null || _regeneratePins)
                            {
                                var cachePin = _mapPinService.TryToGenerateMapPin(url, url, groupCountry, "");
                                if (cachePin == null)
                                {
                                    searchesDone++;
                                    var newUrl = _mapPinService.CheckUrlForMapLinks(url);
                                    cachePin = _mapPinService.TryToGenerateMapPin(newUrl, url, groupCountry, "");
                                }
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

            }

            _databaseLoaderService.SavePinDB();
            _topicsLoaderService.SaveTopics(Topics);
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
        }

        private static string CapitalizeFirstLetter(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        private static string CapitalizeEachWord(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            // Use TextInfo for culture-aware casing
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(str.ToLower());
        }
    }
}
