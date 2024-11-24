using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Gluten.Core.Service
{
    /// <summary>
    /// Some helper function for pin generation
    /// </summary>
    public class PinHelper(Dictionary<string, TopicPinCache> pinCache)
    {
        private readonly Dictionary<string, TopicPinCache> _pinCache = pinCache;

        public TopicPinCache? TryGetPin(string? placeName)
        {
            if (placeName == null) return null;
            if (_pinCache == null) return null;
            if (_pinCache.TryGetValue(placeName, out var value))
                return value;

            foreach (var item in _pinCache.Values)
            {
                if (
                    (item.PlaceName != null && item.PlaceName.StartsWith(placeName, StringComparison.CurrentCultureIgnoreCase))
                    ||
                    (item.Label != null && item.Label.StartsWith(placeName, StringComparison.CurrentCultureIgnoreCase))
                    )
                {
                    return item;
                }
            }
            return null;
        }

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

        public Dictionary<string, TopicPinCache> GetCache()
        {
            return _pinCache;
        }

        /// <summary>
        /// Tries to extract a map location from the geo fields in the url for the centre of the map then tries to location 
        /// the actual location from the data= section
        /// </summary>
        public TopicPinCache? TryToGenerateMapPin(string url, bool onlyFromData, string searchString, string meta)
        {
            if (url == null) return null;
            TopicPinCache? oldPin = TryGetPinByUrl(url);
            url = HttpUtility.UrlDecode(url);
            if (oldPin == null) oldPin = TryGetPinByUrl(url);
            if (oldPin != null) return oldPin;
            var mapsUrl = url;
            //"https://www.google.com/maps/place/7-Eleven Asakusa Kokusaidori Store/data=!4m7!3m6!1s0x60188ebf9036d3b3:0x98f8048bde38bac0!8m2!3d35.713365!4d139.7925459!16s/g/1tgpsmb6!19sChIJs9M2kL-OGGARwLo43osE-Jg?authuser=0&hl=en&rclk=1"
            //https://www.google.com/maps/place/Mister+Donut+Shinjuku+Yasukuni+Street/@37.8507876,125.2890788,5z/data=!3m1!5s0x60188cd981749325:0x7e473d8fd918f3b5!4m10!1m2!2m1!1sMister+Donut,+Japan!3m6!1s0x60188cd981770317:0xd16725fecf632eb7!8m2!3d35.69331!4d139.703677!15sChNNaXN0ZXIgRG9udXQsIEphcGFuIgOIAQFaFCISbWlzdGVyIGRvbnV0IGphcGFukgEKZG9udXRfc2hvcOABAA!16s%2Fg%2F1td5x4gl!5m1!1e4?entry=ttu&g_ep=EgoyMDI0MTExMy4xIKXMDSoASAFQAw%3D%3D

            string lat = "";
            string lon = "";

            var foundInData = TryGetLocationFromDataParameter(url, ref lat, ref lon);
            if (onlyFromData && !foundInData) return null;

            if (url.Contains("/@"))
            {
                var left = url.IndexOf("/@") + 2;
                var latEnd = url.IndexOf(',', left);
                var longEnd = url.IndexOf(',', latEnd + 1);

                if (latEnd > 0)
                {
                    if (!foundInData)
                    {
                        lat = url.Substring(left, latEnd - left);
                        lon = url.Substring(latEnd + 1, longEnd - latEnd - 1);
                    }
                }
            }
            else if (!foundInData)
            {
                return null;
            }

            var placeStart = url.IndexOf("/place/") + "/place/".Length;
            var placeEnd = url.IndexOf('/', placeStart);

            //"https://www.google.com/maps/preview/place/Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyacho,+3%E2%88%9223+onwa/@34.6785478,135.8161308,3281a,13.1y/data\\\\u003d!4m2!3m1!1s0x60013a30562e78d3:0xd712400d34ea1a7b\\"
            //                                          "Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyac"
            var label = url.Substring(placeStart, placeEnd - placeStart);
            label = HttpUtility.UrlDecode(label);

            var newPin = new TopicPinCache()
            {
                Label = label,
                Address = label,
                GeoLatitude = lat,
                GeoLongitude = lon,
                MapsUrl = mapsUrl,
                PlaceName = searchString,
                MetaHtml = meta
            };
            if (!_pinCache.TryGetValue(label, out var existingPin))
            {
                _pinCache.Add(label, newPin);
            }
            else
            {
                existingPin.MapsUrl = newPin.MapsUrl;
            }
            return newPin;
        }

        /// <summary>
        /// Extracts the data= part of a google maps url and looks for the longitude and latitude
        /// </summary>
        public static bool TryGetLocationFromDataParameter(string url, ref string geoLat, ref string geoLong)
        {
            var data = url.Substring(url.IndexOf("data=") + 5);
            if (data.IndexOf('?') > 0)
            {
                data = data.Substring(0, data.IndexOf('?'));
            }
            //!4m6
            //!3m5
            //!1s0x60188dbbed184005:0x88ffa854362ddcfd
            //!8m2
            //!3d35.7278459
            //!4d139.7459932
            //!16s%2Fg%2F1tsbm3f7
            //!5m1
            //!1e4?entry=ttu&g_ep=EgoyMDI0MTAyMS4xIKXMDSoASAFQAw%3D%3D
            //
            //"!4m6!3m5!1s0x6000e7edaea16e25:0xf84a6a950a120ac1!8m2!3d34.6725478!4d135.5034389!16s/g/11h7fg9snj?entry=ttu&g_ep=EgoyMDI0MTAxMy4wIKXMDSoASAFQAw=="
            //!3m2
            //!4b1
            //!5s0x60188be649ab2755:0x641a401c0d2530d6
            //!4m6
            //!3m5
            //!1s0x60188bfa2b1cd833:0x9f8c006e0631372d
            //!8m2
            //!3d35.6699646
            //!4d139.7640287
            //!16s/g/11r7kbm2_b?entry=ttu&g_ep=EgoyMDI0MTAyMS4xIKXMDSoASAFQAw==",

            //!4m9
            //!3m8
            //!1s0x601cb702985745cb:0x215071c6749a7da3
            //!5m2
            //!4m1
            //!1i2
            //!8m2
            //!3d35.5262059
            //!4d137.5673175
            //!16s/g/1tvtbf0y?entry=ttu&g_ep=EgoyMDI0MTAxMy4wIKXMDSoASAFQAw==
            var tokens = data.Split('!');
            var found = false;
            if (tokens.Length > 4)
            {
                foreach (var item in tokens)
                {
                    if (item.StartsWith("3d"))
                    {
                        geoLat = item.Substring(2);
                    }
                    if (item.StartsWith("4d"))
                    {
                        geoLong = item.Substring(2);
                        found = true;
                    }
                }
            }
            return found;
        }
    }
}
