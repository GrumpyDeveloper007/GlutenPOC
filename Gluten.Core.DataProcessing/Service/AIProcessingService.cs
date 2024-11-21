using Azure.AI.TextAnalytics;
using Gluten.Core.DataProcessing.Service.Graveyard;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using System.Web;
using System.Xml.Linq;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Trys to extracted formatted information from human written posts
    /// </summary>
    public class AIProcessingService(
        SeleniumMapsUrlProcessor seleniumMapsUrlProcessor, PinHelper pinHelper, MappingService mappingService)
    {
        private readonly PinHelper _pinHelper = pinHelper;
        private readonly SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor = seleniumMapsUrlProcessor;
        private readonly MappingService _mappingService = mappingService;

        /// <summary>
        /// Uses google maps to search for a place name
        /// </summary>
        public string GetMapUrl(string searchString)
        {
            var searchParameter = HttpUtility.UrlEncode(searchString);
            var mapsLink = $"http://maps.google.com/?q={searchParameter}";
            var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(mapsLink);
            return newUrl;
        }

        /// <summary>
        /// Checks the currently shown page to see if 'permanently closed' is shown
        /// </summary>
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

        /// <summary>
        /// When google maps returns multiple results, this function will try to extact the link for each location
        /// </summary>
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

        /// <summary>
        /// When google maps returns multiple results, this function will try to extact the place name for each result
        /// </summary>
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
        /// Tries to get a pin from the currenty displayed page
        /// </summary>
        public TopicPinCache? GetPinFromCurrentUrl(bool onlyFromData, string restaurantName)
        {
            var newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            var meta = _seleniumMapsUrlProcessor.GetMeta(restaurantName);
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, onlyFromData, restaurantName, meta);
            return pin;
        }

        /// <summary>
        /// Tries to get a pin from the currenty displayed page
        /// </summary>
        public TopicPinCache? GetPinFromCurrentUrl(string newUrl, bool onlyFromData, string restaurantName)
        {
            var meta = _seleniumMapsUrlProcessor.GetMeta(restaurantName);
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, onlyFromData, restaurantName, meta);
            return pin;
        }

        /// <summary>
        /// Gets the current url shown in the browser and tries to extract a geo coordinate
        /// </summary>
        private void UpdatePinList(string newUrl, DetailedTopic topic, ref int urlsCreated)
        {
            newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            var meta = _seleniumMapsUrlProcessor.GetMeta(null);
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, false, newUrl, meta);
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
    }
}
