using Azure.AI.TextAnalytics;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using System.Web;
using System.Xml.Linq;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Trys to extracted formatted information from human written posts
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class AIProcessingService(NaturalLanguageProcessor naturalLanguageProcessor,
        SeleniumMapsUrlProcessor seleniumMapsUrlProcessor, PinHelper pinHelper, MappingService mappingService)
    {
        private readonly NaturalLanguageProcessor _naturalLanguageProcessor = naturalLanguageProcessor;
        private readonly PinHelper _pinHelper = pinHelper;
        private readonly SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor = seleniumMapsUrlProcessor;
        private readonly MappingService _mappingService = mappingService;

        /// <summary>
        /// Processes all the topics in a list
        /// </summary>
        public void AIProcessing(List<DetailedTopic> topics)
        {
            int aiQueries = 0;
            int urlsCreated = 0;
            if (topics == null) return;

            for (int i = 0; i < topics.Count; i++)
            {
                var topic = topics[i];
                if (!topic.HasMapPin() && topic.AiParsed == false)
                {
                    aiQueries++;
                    var restaurantName = "";
                    var newUrl = ProcessTopic(topic, ref restaurantName);
                    if (newUrl != null)
                    {
                        UpdatePinList(newUrl, topic, ref urlsCreated);
                    }
                }
            }
            Console.WriteLine($"AI Queries : {aiQueries}");
            Console.WriteLine($"AI Urls created : {urlsCreated}");
        }

        /// <summary>
        /// Tries to extract location information from a single topic title
        /// </summary>
        public string? ProcessTopic(DetailedTopic topic, ref string restaurantName)
        {
            if (!topic.HasMapPin() && topic.AiParsed == false)
            {
                var aiResponse = _naturalLanguageProcessor.Process(topic.Title);
                restaurantName = "";
                var city = "";
                topic.AiParsed = true;
                foreach (var item in aiResponse)
                {
                    if (item.Category == EntityCategory.Location && item.SubCategory == null)
                    {
                        restaurantName = item.Text;
                    }
                    if (item.Category == EntityCategory.Location && item.SubCategory == "City")
                    {
                        city = item.Text;
                    }

                    /*
                     * TODO: Not required at the moment, remove?
                    if (topic.AiTitleInfoV2 == null) topic.AiTitleInfoV2 = new List<AiInformation>();
                    topic.AiTitleInfoV2.Add(new AiInformation()
                    {
                        Text = item.Text,
                        Category = item.Category.ToString(),
                        SubCategory = item.SubCategory,
                        ConfidenceScore = item.ConfidenceScore,
                        Length = item.Length,
                        Offset = item.Offset
                    });
                    */
                }

                // Skip if all we have is the city name
                if (!string.IsNullOrEmpty(restaurantName))
                {
                    return GetMapUrl($"{restaurantName},{city}");
                }
            }

            return null;

        }

        public string GetMapUrl(string searchString)
        {
            var searchParameter = HttpUtility.UrlEncode(searchString);
            var mapsLink = $"http://maps.google.com/?q={searchParameter}";
            // TODO: Hacks
            var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(mapsLink);
            return newUrl;
        }

        public bool IsPermanentlyClosed(string? placeName, out string meta)
        {
            meta = "";
            if (placeName == null) return true;
            var r = _seleniumMapsUrlProcessor.GetSearchResults();
            foreach (var item in r)
            {
                var innerText = item.Text;
                var a = item.GetAttribute("aria-label");
                if (placeName == a)
                {
                    meta = item.GetAttribute("innerHTML");
                }
                if (innerText.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetMapUrls()
        {
            var results = new List<string>();
            var r = _seleniumMapsUrlProcessor.GetSearchResults();
            foreach (var item in r)
            {
                var innerText = item.Text;
                var b = item.GetAttribute("href");

                if (!string.IsNullOrWhiteSpace(b)
                    && !innerText.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                {
                    //https://www.google.com/maps/contrib/117521353174744275953?hl=en-AU
                    //https://www.google.com/maps/place/Rizlabo+Kitchen/@35.6685791,139.708229,17z/data=!4m6!3m5!1s0x60188ca21d3193d9:0x9127dcb2b56f681e!8m2!3d35.6685791!4d139.7108039!16s%2Fg%2F11c5xc_56p?entry=ttu&g_ep=EgoyMDI0MTExMy4xIKXMDSoASAFQAw%3D%3D
                    //https://www.google.com/maps/place/Mister+Donut+Shinjuku+Yasukuni+Street/@35.69331,139.703677,17z/data=!3m1!4b1!4m6!3m5!1s0x60188cd981770317:0xd16725fecf632eb7!8m2!3d35.69331!4d139.703677!16s%2Fg%2F1td5x4gl!5m1!1e4?authuser=0&hl=en&entry=ttu&g_ep=EgoyMDI0MTExMy4xIKXMDSoASAFQAw%3D%3D
                    if (b.StartsWith("https://www.google.com/maps/place"))
                    {
                        //var a = item.GetAttribute("aria-label");
                        results.Add(b);
                    }
                }
            }

            return results;
        }

        public List<string> GetMapPlaceNames()
        {
            var results = new List<string>();
            var r = _seleniumMapsUrlProcessor.GetSearchResults();
            foreach (var item in r)
            {
                var innerText = item.Text;
                var b = item.GetAttribute("href");
                if (!string.IsNullOrWhiteSpace(b)
                    && !innerText.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (b.StartsWith("https://www.google.com/maps/place"))
                    {
                        var a = item.GetAttribute("aria-label");
                        results.Add(a);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the current url shown in the browser and tries to extract a geo coordinate
        /// </summary>
        public void UpdatePinList(string newUrl, DetailedTopic topic, ref int urlsCreated)
        {
            newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, false, newUrl);
            if (pin != null)
            {
                urlsCreated++;
                topic.UrlsV2 ??= [];
                topic.UrlsV2.Add(new TopicLink()
                {
                    AiGenerated = true,
                    Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin),
                    Url = newUrl

                });
            }
        }

        public TopicPinCache? GetPinFromCurrentUrl(bool onlyFromData, string restaurantName)
        {
            var newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, onlyFromData, restaurantName);
            return pin;
        }

        public TopicPinCache? GetPinFromCurrentUrl(string newUrl, bool onlyFromData, string restaurantName)
        {
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, onlyFromData, restaurantName);
            return pin;
        }

    }
}
