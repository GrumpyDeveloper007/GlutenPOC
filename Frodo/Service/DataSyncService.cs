using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System.Web;
using StringHelper = Frodo.Helper.StringHelper;

namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// All the magic happens here
    /// </summary>
    internal class DataSyncService(MapPinService _mapPinService,
        PinHelper _pinHelper,
        DatabaseLoaderService _databaseLoaderService,
        MappingService _mappingService,
        ClientExportFileGenerator _clientExportFileGenerator,
        GeoService _geoService,
        FBGroupService _fBGroupService,
        MapPinCache _mapPinCache
        )
    {
        private readonly TopicsHelper _topicsHelper = new();
        private readonly LocalAiInterfaceService _analyzeDocumentService = new();
        private readonly TopicLoaderService _topicLoaderService = new();
        private readonly MapsMetaExtractorService _mapsMetaExtractorService = new();
        private readonly FBGroupService _fbGroupService = new();

        public List<DetailedTopic> Topics = [];
        private readonly bool _regeneratePins = true;
        private int _invalidGeo = 0;
        private List<AiVenue> _placeNameSkipList = [];

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

            _placeNameSkipList = _databaseLoaderService.LoadPlaceSkipList();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nReading captured FB data");
            //_topicLoaderService.ReadFileLineByLine("D:\\Coding\\Gluten\\Smeagol\\bin\\Debug\\net8.0\\Responses.txt", Topics);

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
            //UpdatePinsForAiVenues();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nExtracting meta info for pins");
            ExtractMetaInfoFromPinCache();

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nFiltering AI pins");
            RemoveAiPinsInBadLocations();


            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nGenerating data for client application");
            _clientExportFileGenerator.GenerateTopicExport(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
        }

        private void GenerateShortTitles()
        {
            int shortTitlesAdded = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                if (i % 10 == 0)
                    Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                if (string.IsNullOrWhiteSpace(topic.ShortTitle) && !topic.ShortTitleProcessed)
                {
                    topic.ShortTitle = _analyzeDocumentService.GenerateShortTitle(topic.Title);
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

        private void ExtractMetaInfoFromPinCache()
        {
            int i = 0;
            var cache = _mapPinCache.GetCache();
            _mapsMetaExtractorService.ClearRestaurantType();
            foreach (var item in cache)
            {
                if (string.IsNullOrWhiteSpace(item.Value.Country))
                {
                    double longitude = double.Parse(item.Value.GeoLongitude);
                    double latitude = double.Parse(item.Value.GeoLatitude);
                    item.Value.Country = _geoService.GetCounty(longitude, latitude);
                }
                Console.WriteLine($"Processing pin meta {i} of {cache.Count}");

                //if (!item.Value.MetaProcessed)
                {
                    if (string.IsNullOrWhiteSpace(item.Value.MetaHtml) && item.Value.MapsUrl != null)
                    {
                        // load meta if missing
                        var url2 = _mapPinService.GoAndWaitForUrlChange(item.Value.MapsUrl);
                        url2 = _mapPinService.GetCurrentUrl();
                        url2 = HttpUtility.UrlDecode(url2);

                        var newPin = _pinHelper.GenerateMapPin(url2, item.Value.Label, "");
                        if (newPin != null)
                            item.Value.Label = newPin.Label;

                        item.Value.MetaHtml = _mapPinService.GetMeta(item.Value.Label);
                        if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                        {
                            var placeNames = _mapPinService.GetMapPlaceNames();
                            if (placeNames.Count > 0)
                            {
                                cache.Remove(item.Key);
                                Console.WriteLine($"Cache pin returns multiple results {item.Value.Label}");
                                continue;
                            }
                            if (placeNames.Count == 0)
                            {
                                cache.Remove(item.Key);
                                Console.WriteLine($"0 results {item.Value.Label}");
                            }

                        }

                        if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                        {
                            var url = _mapPinService.GetMapUrl(item.Value.Label);
                            var placeNames = _mapPinService.GetMapPlaceNames();
                            if (placeNames.Count > 0)
                            {
                                cache.Remove(item.Key);
                                Console.WriteLine($"Multiple results {item.Value.Label}");
                            }
                            if (placeNames.Count == 0)
                            {
                                //cache.Remove(item.Key);
                                Console.WriteLine($"0 results {item.Value.Label}");
                            }

                            item.Value.MapsUrl = url;
                            var pin = _pinHelper.GenerateMapPin(url, "", "");
                            item.Value.MetaHtml = _mapPinService.GetMeta(item.Value.Label);
                            if (pin != null && string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                            {
                                item.Value.MetaHtml = _mapPinService.GetMeta(pin.Label);
                            }
                            if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                            {
                                Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                            }
                        }

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
                    item.Value.MetaProcessed = true;
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
                }
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
                Console.WriteLine($"Processing {i} of {Topics.Count}");
                var topic = Topics[i];
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
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

            Console.WriteLine($"Has Links : {linkCount}");
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
            Console.WriteLine($"Has Response Links : {responseLinkCount}");
            Console.WriteLine($"Has Response Maps Links : {responseMapsLinkCount}");
        }

        private void RemoveAiPinsInBadLocations()
        {
            var restaurantService = new RestaurantTypeService();
            _invalidGeo = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                if (i % 100 == 0)
                    Console.WriteLine($"Processing AI pins {i} of {Topics.Count}");

                if (topic.AiVenues != null)
                {
                    for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
                    {
                        var venue = topic.AiVenues[t];
                        var pin = topic.AiVenues[t].Pin;
                        if (pin != null)
                        {
                            var cachePin = _mapPinCache.TryGetPinLatLong(pin.GeoLatitude, pin.GeoLongitude);
                            var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                            var country = _geoService.GetCountryPin(cachePin);

                            if (cachePin != null)
                            {
                                if (cachePin.MetaData != null && cachePin.MetaData.PermanentlyClosed)
                                {
                                    Console.WriteLine($"Removing pin {t} in topic {i} - PermanentlyClosed");
                                    topic.AiVenues.RemoveAt(t);
                                }
                                else if (!IsPinInGroupCountry(pin, topic))
                                {
                                    Console.WriteLine($"Removing pin {t} in topic {i} - country mismatch {groupCountry}, {country}");
                                    topic.AiVenues.RemoveAt(t);
                                }
                                else if (restaurantService.IsRejectedRestaurantType(cachePin.MetaData?.RestaurantType))
                                {
                                    //TODO for debug - _mapPinService.GoToUrl(cachePin.MapsUrl);
                                    Console.WriteLine($"Removing pin {t} in topic {i} - {cachePin.MetaData.RestaurantType}");
                                    topic.AiVenues.RemoveAt(t);
                                }
                            }
                        }

                    }
                }
            }

            Console.WriteLine($"Invalid pins {_invalidGeo}");


            _topicsHelper.SaveTopics(Topics);
        }

        private bool IsPinInGroupCountry(TopicPin? pin, DetailedTopic topic)
        {
            if (pin != null)
            {
                double longitude = double.Parse(pin.GeoLongitude);
                double latitude = double.Parse(pin.GeoLatitude);
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                var country = _geoService.GetCounty(longitude, latitude);
                if (!string.IsNullOrWhiteSpace(country))
                {
                    if (groupCountry != country
                        && !(groupCountry == "Hong Kong" && country == "China"))
                    {
                        Console.WriteLine($"country mismatch, group: {groupCountry} pin: {country}, {latitude}, {longitude}");
                        return false;
                    }
                }
                else
                {
                    _invalidGeo++;
                }
            }
            return true;
        }

        private static string? Truncate(string? value, int maxLength, string truncationSuffix = "…")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }

        /// <summary>
        /// Scan generated Ai Venues and try to generate any missing pins
        /// </summary>
        private void UpdatePinsForAiVenues()
        {
            var aiPin = new AiVenueProcessorService(_mapPinService, _mappingService, _fbGroupService);

            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                var groupId = topic.GroupId;

                if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;

                Console.WriteLine($"Update Pins For AI Venues {i} of {Topics.Count} : {Truncate(topic.Title, 75).Replace("\n", "")}");

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

                var chainUrls = new List<string>();
                for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
                {
                    AiVenue? ai = topic.AiVenues[t];
                    // Only process unprocessed pins

                    var cachedPin = _mapPinCache.TryGetPin(ai.PlaceName, groupCountry);
                    if (cachedPin != null)
                    {
                        ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
                        continue;
                    }

                    // alternate search
                    if (ai.Pin != null)
                    {
                        cachedPin = _mapPinCache.TryGetPin(ai.Pin.Label, groupCountry);
                        if (cachedPin != null)
                        {
                            ai.Pin = _mappingService.Map<TopicPin, TopicPinCache>(cachedPin);
                            continue;
                        }
                    }

                    if (ai.Pin == null && !ai.PinSearchDone || _regeneratePins)
                    {
                        if (ai.PlaceName != null
                            && ai.Address != null
                            && _placeNameSkipList.Exists(o =>
                            (o.PlaceName.Contains(ai.PlaceName, StringComparison.InvariantCultureIgnoreCase)
                            && o.Address != null && o.Address.Contains(ai.Address, StringComparison.InvariantCultureIgnoreCase))))
                        {
                            Console.WriteLine($"Skipping : {topic.AiVenues[t].PlaceName}");
                        }
                        else
                        {
                            if (ai.Pin == null || _regeneratePins)
                            {
                                aiPin.GetMapPinForPlaceName(ai, groupId, chainUrls);
                            }
                            if (ai.Pin == null)
                            {
                                // drop pin
                                if (!string.IsNullOrWhiteSpace(topic.AiVenues[t].PlaceName)
                                    && chainUrls.Count == 0)
                                {
                                    _placeNameSkipList.Add(topic.AiVenues[t]);
                                    Console.WriteLine($"Tagging pin as skippable : {topic.AiVenues[t].PlaceName}");
                                    //topic.AiVenues[t].PlaceName = "";
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Found pin : {topic.AiVenues[t].PlaceName}");

                            }
                        }

                    }

                    if (ai.Pin != null)
                    {
                        if (!IsPinInGroupCountry(ai.Pin, topic))
                        {
                            Console.WriteLine($"Removing pin due to country : {topic.AiVenues[t].PlaceName}");
                            topic.AiVenues.RemoveAt(t);
                        }
                    }
                    ai.PinSearchDone = true;
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
                foreach (var url in chainUrls)
                {
                    var pin = _pinHelper.GenerateMapPin(url, "", groupCountry);
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
            _topicsHelper.SaveTopics(Topics);
            _databaseLoaderService.SavePinDB();
            _databaseLoaderService.SavePlaceSkipList(_placeNameSkipList);
        }

    }
}
