using Gluten.Core.Helper;
using Gluten.Data.PinCache;
using System;
using System.Web;
using Gluten.Core.LocationProcessing.Helper;
using Gluten.Core.Interface;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Google map pin handling service, generates pins from place names, gathers meta data
    /// </summary>
    public class MapPinService(
        SeleniumService _seleniumMapsUrlProcessor,
        MapPinCacheService _mapPinCache,
        GeoService _geoService,
        MapsMetaExtractorService _mapsMetaExtractorService,
        IConsole Console)
    {

        /// <summary>
        /// Uses google maps to search for a place name
        /// </summary>
        public string GetMapUrl(string searchString)
        {
            var searchParameter = HttpUtility.UrlEncode(searchString);
            var mapsLink = $"http://maps.google.com/?q={searchParameter}";
            var newUrl = CheckUrlForMapLinks(mapsLink);
            return newUrl;
        }

        /// <summary>
        /// Gos to the specified url and waits for a change in the url (used for maps)
        /// </summary>
        public string GoAndWaitForUrlChange(string url)
        {
            return _seleniumMapsUrlProcessor.GoAndWaitForUrlChange(url);
        }

        /// <summary>
        /// Gets meta data shown on the current page
        /// </summary>
        public string GetMeta(string? placeName)
        {
            if (placeName == null) return "";
            var placeNameWithoutAccents = StringHelper.RemoveIrrelevantChars(StringHelper.RemoveDiacritics(placeName));
            var r = _seleniumMapsUrlProcessor.GetSearchResults();
            foreach (var item in r)
            {
                var a = item.GetAttribute("aria-label");
                if (a.StartsWith(placeName, StringComparison.InvariantCultureIgnoreCase)
                    || placeName.StartsWith(a, StringComparison.InvariantCultureIgnoreCase)
                    || StringHelper.RemoveIrrelevantChars(StringHelper.RemoveDiacritics(a)).StartsWith(placeNameWithoutAccents, StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    return item.GetAttribute("innerHTML");
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the current url in the browser
        /// </summary>
        public string GetCurrentUrl()
        {
            return _seleniumMapsUrlProcessor.GetCurrentUrl();
        }

        /// <summary>
        /// Checks if a url is a valid google maps link, waits for the url to be updated to include the location
        /// </summary>
        public string CheckUrlForMapLinks(string url)
        {
            if (MapPinHelper.IsMapsUrl(url))
            {
                if (!url.Contains("/@"))
                {
                    GoAndWaitForUrlChange(url);
                    var newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
                    newUrl = HttpUtility.UrlDecode(newUrl);
                    int timeout = 50;
                    while (!newUrl.Contains("/@") && timeout > 0)
                    {
                        timeout--;
                        Thread.Sleep(200);
                        newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
                        newUrl = HttpUtility.UrlDecode(newUrl);
                        var pageSource = _seleniumMapsUrlProcessor.PageSource();
                        if (pageSource.Contains("This site can’t be reached"))
                        {
                            GoAndWaitForUrlChange(url);
                        }
                    }
                    if (timeout == 0 && !newUrl.Contains("/@"))
                    {
                        Console.WriteLine("Timeout waiting for url");
                        newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
                        newUrl = HttpUtility.UrlDecode(newUrl);
                    }

                    return newUrl;
                }
            }
            return url;
        }

        /// <summary>
        /// Gets the html in the first interesting label
        /// </summary>
        public string GetFirstLabelInnerHTML()
        {
            return _seleniumMapsUrlProcessor.GetFirstLabelInnerHTML();
        }

        /// <summary>
        /// Go’s to the specified url
        /// </summary>
        /// <param name="url"></param>
        public void GoToUrl(string url)
        {
            _seleniumMapsUrlProcessor.GoToUrl(url);
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
            int i = 0;
            while (r.Count == 0 && i < 20)
            {
                Thread.Sleep(100); // TODO: Hack, page still loading
                r = _seleniumMapsUrlProcessor.GetSearchResults();
                i++;
            }
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
        public TopicPinCache? GetPinFromCurrentUrl(string restaurantName, string originalPlaceName)
        {
            var newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            return TryToGenerateMapPin(newUrl, restaurantName, "", originalPlaceName);
        }

        /// <summary>
        /// Tries to convert the specified url in to a map pin
        /// </summary>
        public TopicPinCache? TryToGenerateMapPin(string url, string searchString, string country, string originalPlaceName)
        {
            var pin = _mapPinCache.TryToGenerateMapPin(url, searchString, country, originalPlaceName);

            if (pin != null)
            {
                pin.Country = _geoService.GetCountryPin(pin);
                var metaHtml = _mapPinCache.GetMetaHtml(pin.GeoLatitude, pin.GeoLongitude);
                if (string.IsNullOrWhiteSpace(metaHtml))
                {
                    metaHtml = GetMeta(pin.Label);
                }
                if (!string.IsNullOrWhiteSpace(metaHtml))
                {
                    _mapPinCache.AddUpdateMetaHtml(metaHtml, pin.GeoLatitude, pin.GeoLongitude);
                    pin.MetaData = _mapsMetaExtractorService.ExtractMeta(metaHtml);
                }
            }

            return pin;
        }
    }
}
