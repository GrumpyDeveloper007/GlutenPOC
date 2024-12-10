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
    public class MapPinCache(
        Dictionary<string,
        TopicPinCache> pinCache,
        IConsole Console)
    {
        private Dictionary<string, TopicPinCache> _pinCache = pinCache;

        /// <summary>
        /// Tries to get a cached pin based on the place name, with fuzzy logic
        /// </summary>
        public TopicPinCache? TryGetPin(string? placeName, string country)
        {
            if (placeName == null) return null;
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
        public TopicPinCache? TryGetPinLatLong(string? latitude, string? longitude)
        {
            if (latitude == null) return null;
            if (longitude == null) return null;
            if (_pinCache == null) return null;

            foreach (var item in _pinCache.Values)
            {

                if (item.GeoLatitude == latitude
                     && item.GeoLongitude == longitude)
                {
                    return item;
                }
            }
            return null;
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
        /// Updates the database
        /// </summary>
        public void SetCache(Dictionary<string, TopicPinCache> newCache)
        {
            _pinCache = newCache;
        }

        /// <summary>
        /// Tries to extract a map location from the geo fields in the url for the centre of the map then tries to location 
        /// the actual location from the data= section
        /// </summary>
        public TopicPinCache? TryToGenerateMapPin(string url, string searchString, string country)
        {
            if (url == null) return null;
            TopicPinCache? oldPin = TryGetPinByUrl(url);
            url = HttpUtility.UrlDecode(url);
            if (oldPin != null) return oldPin;
            var newPin = PinHelper.GenerateMapPin(url, searchString, country);

            if (newPin != null)
            {
                var existingPin = TryGetPin(newPin.Label, country);

                existingPin ??= TryGetPinLatLong(newPin.GeoLatitude, newPin.GeoLongitude);
                if (existingPin == null)
                {
                    //newPin.Country = country;
                    if (newPin.Label != null)
                    {
                        _pinCache.Add(newPin.Label, newPin);
                        Console.WriteLine($"Adding cache pin :'{newPin.Label}");
                    }
                }
                else
                {
                    Console.WriteLine($"Adding search parameter :'{searchString}' to : '{newPin.Label}'");
                    existingPin.SearchStrings.Add(searchString);
                }
            }
            return newPin;
        }


    }
}
