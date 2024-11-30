using Gluten.Core.Helper;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System.Diagnostics.Metrics;
using System;
using System.Web;
using System.Xml.Linq;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Google map pin handling service, generates pins from place names, gathers meta data
    /// </summary>
    public class MapPinService(
        SeleniumMapsUrlProcessor seleniumMapsUrlProcessor, PinHelper pinHelper,
        MapPinCache _mapPinCache,
        GeoService _geoService,
        MapsMetaExtractorService _mapsMetaExtractorService)
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
            var newUrl = CheckUrlForMapLinks(mapsLink);
            return newUrl;
        }

        public string GoAndWaitForUrlChange(string url)
        {
            return _seleniumMapsUrlProcessor.GoAndWaitForUrlChange(url);
        }

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

        public string GetCurrentUrl()
        {
            return _seleniumMapsUrlProcessor.GetCurrentUrl();
        }

        private bool IsMapsUrl(string url)
        {
            // Link to street view
            //https://maps.google.com/maps/api/staticmap?center=34.6845322%2C135.1840363&amp;zoom=-1&amp;size=900x900&amp;language=en&amp;sensor=false&amp;client=google-maps-frontend&amp;signature=yGPXtu3-Vjroz_DtJZLPyDkVVC8\
            // Collection of pins 
            //https://www.google.com/maps/d/viewer?mid=16xtxMz-iijlDOEl-dlQKEa2-A19nxzND&ll=35.67714795882308,139.72588715&z=12
            if (!url.Contains("https://www.google.com/maps/d/viewer")
                && (url.Contains("www.google.com/maps/")
                || url.Contains("maps.app.goo.gl")
                || url.Contains("maps.google.com")
                || url.Contains("https://goo.gl/maps/"))
                )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a url is a valid google maps link, waits for the url to be updated to include the location
        /// </summary>
        public string CheckUrlForMapLinks(string url)
        {
            if (IsMapsUrl(url))
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

        public string GetFirstLabelInnerHTML()
        {
            return _seleniumMapsUrlProcessor.GetFirstLabelInnerHTML();
        }

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
        public TopicPinCache? GetPinFromCurrentUrl(string restaurantName)
        {
            var newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            return TryToGenerateMapPin(newUrl, restaurantName, "");
        }

        public TopicPinCache? TryToGenerateMapPin(string url, string searchString, string country)
        {
            var pin = _mapPinCache.TryToGenerateMapPin(url, searchString, "");

            if (pin != null)
            {
                double longitude = double.Parse(pin.GeoLongitude);
                double latitude = double.Parse(pin.GeoLatitude);
                country = _geoService.GetCounty(longitude, latitude);
                pin.Country = _geoService.GetCounty(longitude, latitude);
            }

            if (pin != null && string.IsNullOrWhiteSpace(pin.MetaHtml))
            {
                if (string.IsNullOrWhiteSpace(pin.MetaHtml))
                {
                    pin.MetaHtml = GetMeta(pin.Label);
                }
                if (string.IsNullOrWhiteSpace(pin.MetaHtml))
                {
                    pin.MetaData = _mapsMetaExtractorService.ExtractMeta(pin.MetaHtml);
                }
            }

            return pin;

        }
    }
}
