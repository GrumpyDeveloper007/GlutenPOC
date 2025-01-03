// Ignore Spelling: Lat

using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Data.PinCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Provides caching of pins
    /// </summary>
    public class MapPinCacheService(
        Dictionary<string, TopicPinCache> pinCache,
        Dictionary<string, PinCacheMetaHtml> pinCacheHtml,
        IConsole Console)
    {
        private readonly Dictionary<string, TopicPinCache> _pinCache = pinCache;
        private readonly Dictionary<string, PinCacheMetaHtml> _pinCacheMeta = pinCacheHtml;

        public bool SavePinCacheHtml { get; set; }

        public void Clean()
        {
            int count = 0;
            foreach (var item in _pinCache.Values)
            {
                count++;
                if (item == null) continue;
                for (int i = item.SearchStrings.Count - 1; i >= 0; i--)
                {
                    // remove duplicates
                    for (int t = item.SearchStrings.Count - 1; t >= 0; t--)
                    {
                        if (item.SearchStrings[t] == item.SearchStrings[i] && t != i)
                        {
                            Console.WriteLine($"Removing {item.SearchStrings[t]}");
                            item.SearchStrings.RemoveAt(i);
                            break;
                        }
                    }

                    if (item.SearchStrings[i].StartsWith("https://www.google.com/maps/place/")
                        || item.SearchStrings[i].StartsWith("https://www.google.it/maps/place/"))
                    {
                        Console.WriteLine($"Removing {item.SearchStrings[i]}");
                        item.SearchStrings.RemoveAt(i);
                    }

                }
            }
        }

        /// <summary>
        /// Tries to get a cached pin based on the place name, with fuzzy logic
        /// </summary>
        public TopicPinCache? TryGetPin(string? placeName, string country)
        {
            if (string.IsNullOrWhiteSpace(placeName)) return null;
            if (_pinCache == null) return null;
            if (_pinCache.TryGetValue(placeName, out var value))
                return value;

            var searchPlace = StringHelper.RemoveDiacritics(StringHelper.RemoveIrrelevantChars(placeName));

            foreach (var item in _pinCache.Values)
            {
                var itemPlace = StringHelper.RemoveDiacritics(StringHelper.RemoveIrrelevantChars(item.PlaceName ?? ""));
                var itemLabel = StringHelper.RemoveDiacritics(StringHelper.RemoveIrrelevantChars(item.Label ?? ""));

                foreach (var searchString in item.SearchStrings)
                {
                    if (searchString.StartsWith(placeName)) return item;
                }

                if (
                    itemPlace != null && (itemPlace.StartsWith(searchPlace, StringComparison.CurrentCultureIgnoreCase)
                    || item.PlaceName != null && itemPlace.StartsWith(placeName, StringComparison.CurrentCultureIgnoreCase)
                    || itemLabel != null && itemLabel.StartsWith(searchPlace, StringComparison.CurrentCultureIgnoreCase)
                    || itemLabel != null && item.Label != null && itemLabel.StartsWith(placeName, StringComparison.CurrentCultureIgnoreCase))
                    )
                {
                    if (item.Country == country || item.Country == "" || country == "")
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to get a pin based on Geo location
        /// </summary>
        public TopicPinCache? TryGetPinLatLong(string? latitude, string? longitude, string searchString)
        {
            TopicPinCache? foundItem = null;
            if (latitude == null) return null;
            if (longitude == null) return null;
            if (_pinCache == null) return null;

            foreach (var item in _pinCache.Values)
            {
                if (item.GeoLatitude == latitude
                     && item.GeoLongitude == longitude)
                {
                    if (item.SearchStrings.Contains(searchString)
                        || string.IsNullOrWhiteSpace(searchString)
                        || item.SearchStrings.Any(o => HttpUtility.UrlDecode(o).ToLower().Replace(" ", "") == searchString.ToLower().Replace(" ", "")))
                    {
                        foundItem = item;
                        return foundItem;
                    }
                }
            }

            return foundItem;
        }

        public TopicPinCache AddPinCache(TopicPinCache newPin, string searchString, string originalPlaceName)
        {
            var oldPin = TryGetPinLatLong(newPin.GeoLatitude, newPin.GeoLongitude, "");
            if (oldPin != null)
            {
                Console.WriteLineRed($"pin found in cache name :{oldPin.Label}, adding search string :{originalPlaceName}");
                if (!string.IsNullOrWhiteSpace(originalPlaceName) && !oldPin.SearchStrings.Contains(originalPlaceName)) oldPin.SearchStrings.Add(originalPlaceName);
                if (!string.IsNullOrWhiteSpace(searchString) && !oldPin.SearchStrings.Contains(searchString)) oldPin.SearchStrings.Add(searchString);
                return oldPin;
            }
            else
            {
                if (_pinCache.TryGetValue(newPin.Label, out TopicPinCache? value))
                {
                    oldPin = value;
                    Console.WriteLineRed($"Existing cache entry error: {newPin.GeoLatitude}, {newPin.GeoLongitude} old: {oldPin.GeoLatitude},{oldPin.GeoLongitude}");
                    if (newPin.GeoLatitude == oldPin.GeoLatitude || newPin.GeoLongitude == oldPin.GeoLongitude)
                    {
                        return oldPin;
                    }
                    _pinCache.Add($"{newPin.Label}{newPin.GeoLatitude}:{newPin.GeoLongitude}", newPin);
                    return newPin;
                }
                else
                {
                    _pinCache.Add(newPin.Label, newPin);
                    return newPin;
                }
            }
        }

        /// <summary>
        /// Tries to get a pin based on a url
        /// </summary>
        public TopicPinCache? TryGetPinByUrl(string url)
        {
            if (_pinCache == null) return null;

            foreach (var item in _pinCache.Values)
            {
                if (item.MapsUrl != null && item.MapsUrl.StartsWith(url, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item;
                }

                foreach (var searchItem in item.SearchStrings)
                {
                    if (searchItem == url)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the current state of the database
        /// </summary>
        public Dictionary<string, TopicPinCache> GetCache()
        {
            return _pinCache;
        }

        /// <summary>
        /// Gets the current state of the Html store database
        /// </summary>
        public Dictionary<string, PinCacheMetaHtml> GetCacheHtml()
        {
            return _pinCacheMeta;
        }

        public void AddUpdateMetaHtml(string? html, string geoLatitude, string geoLongitude)
        {
            if (string.IsNullOrWhiteSpace(html)) return;
            if (!_pinCacheMeta.TryGetValue($"{geoLatitude}:{geoLongitude}", out var _))
            {
                SavePinCacheHtml = true;
                _pinCacheMeta.Add($"{geoLatitude}:{geoLongitude}", new PinCacheMetaHtml
                {
                    GeoLatitude = geoLatitude,
                    GeoLongitude = geoLongitude,
                    MetaHtml = html
                });
            }
        }

        public string GetMetaHtml(string geoLatitude, string geoLongitude)
        {
            if (_pinCacheMeta.TryGetValue($"{geoLatitude}:{geoLongitude}", out var value))
            {
                return value.MetaHtml;
            }
            return "";
        }

        /// <summary>
        /// Tries to extract a map location from the geo fields in the url for the centre of the map then tries to location 
        /// the actual location from the data= section
        /// </summary>
        public TopicPinCache? TryToGenerateMapPin(string url, string searchString, string country, string originalPlaceName)
        {
            if (url == null) return null;
            TopicPinCache? oldPin = TryGetPinByUrl(url);
            url = HttpUtility.UrlDecode(url);
            var newPin = PinHelper.GenerateMapPin(url, searchString, country);

            if (newPin == null) return null;
            return AddPinCache(newPin, searchString, originalPlaceName);
        }


    }
}
