using Gluten.Core.Helper;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System.Web;
using System.Xml.Linq;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Google map pin handling service, generates pins from place names, gathers meta data
    /// </summary>
    public class MapPinService(
        SeleniumMapsUrlProcessor seleniumMapsUrlProcessor, PinHelper pinHelper)
    {
        private readonly PinHelper _pinHelper = pinHelper;
        private readonly SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor = seleniumMapsUrlProcessor;

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
        public bool IsPermanentlyClosed(string? placeName)
        {
            if (placeName == null) return true;
            var innerHtml = _seleniumMapsUrlProcessor.GetFirstLabelInnerHTML();
            if (innerHtml.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// When google maps returns multiple results, this function will try to extract the link for each location
        /// </summary>
        public List<string> GetMapUrls()
        {
            var results = new List<string>();
            var r = _seleniumMapsUrlProcessor.GetSearchResults();
            foreach (var item in r)
            {
                var b = item.GetAttribute("href");
                if (!string.IsNullOrWhiteSpace(b))
                {
                    var innerText = item.Text;
                    if (!innerText.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (b.StartsWith("https://www.google.com/maps/place"))
                        {
                            results.Add(b);
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// When google maps returns multiple results, this function will try to extract the place name for each result
        /// </summary>
        public List<string> GetMapPlaceNames()
        {
            var results = new List<string>();
            var r = _seleniumMapsUrlProcessor.GetSearchResults();
            foreach (var item in r)
            {
                var b = item.GetAttribute("href");
                if (!string.IsNullOrWhiteSpace(b))
                {
                    var innerText = item.Text;
                    if (!innerText.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (b.StartsWith("https://www.google.com/maps/place"))
                        {
                            var a = item.GetAttribute("aria-label");
                            results.Add(a);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Tries to get a pin from the currently displayed page
        /// </summary>
        public TopicPinCache? GetPinFromCurrentUrl(bool onlyFromData, string restaurantName)
        {
            var newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, onlyFromData, restaurantName);
            if (pin != null && string.IsNullOrWhiteSpace(pin.MetaHtml))
            {
                pin.MetaHtml = _seleniumMapsUrlProcessor.GetMeta(pin.Label);
            }
            return pin;
        }

        /// <summary>
        /// Tries to get a pin from the currently displayed page
        /// </summary>
        public TopicPinCache? GetPinFromCurrentUrl(string newUrl, bool onlyFromData, string restaurantName)
        {
            var pin = _pinHelper.TryToGenerateMapPin(newUrl, onlyFromData, restaurantName);
            if (pin != null)
            {
                pin.MetaHtml = _seleniumMapsUrlProcessor.GetMeta(pin.Label);
            }
            return pin;
        }
    }
}
