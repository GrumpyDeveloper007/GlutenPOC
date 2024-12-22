// Ignore Spelling: geo

using Gluten.Data.ClientModel;
using Gluten.Data.PinCache;

namespace Gluten.Core.Helper
{
    /// <summary>
    /// Some helper function for pin generation
    /// </summary>
    public class PinHelper
    {
        /// <summary>
        /// Checks to see if there is a pin in the list that has the Latitude/Longitude
        /// </summary>
        public static bool IsInList(List<PinTopic> pins, double geoLatitude, double geoLongitude)
        {
            foreach (var pin in pins)
            {
                if (pin.GeoLatitude == geoLatitude && pin.GeoLongitude == geoLongitude) { return true; }
            }
            return false;
        }

        public static string GetPlaceNameFromSearchUrl(string url)
        {
            var placeStart = url.IndexOf("/search/") + "/search/".Length;
            var placeEnd = url.IndexOf('/', placeStart);
            return url.Substring(placeStart, placeEnd - placeStart);
        }

        /// <summary>
        /// Converts a url to a Pin cache data item
        /// </summary>
        public static TopicPinCache? GenerateMapPin(string url, string? searchString, string country)
        {
            if (url == null) return null;
            var mapsUrl = url;

            string lat = "";
            string lon = "";

            var foundInData = TryGetLocationFromDataParameter(url, ref lat, ref lon);
            if (!foundInData) return null;

            var placeStart = url.IndexOf("/place/") + "/place/".Length;
            var placeEnd = url.IndexOf('/', placeStart);

            //"https://www.google.com/maps/preview/place/Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyacho,+3%E2%88%9223+onwa/@34.6785478,135.8161308,3281a,13.1y/data\\\\u003d!4m2!3m1!1s0x60013a30562e78d3:0xd712400d34ea1a7b\\"
            //                                          "Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyac"
            var label = url.Substring(placeStart, placeEnd - placeStart);

            var newPin = new TopicPinCache()
            {
                Label = label,
                GeoLatitude = lat,
                GeoLongitude = lon,
                MapsUrl = mapsUrl,
                PlaceName = searchString,
                Country = country,
            };
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                newPin.SearchStrings.Add(searchString);
            }
            else
            {

                newPin.PlaceName = System.Uri.UnescapeDataString(newPin.Label.Replace("+", " "));
            }
            return newPin;
        }


        /// <summary>
        /// Extracts the data= part of a google maps url and looks for the longitude and latitude
        /// </summary>
        public static bool TryGetLocationFromDataParameter(string url, ref string geoLatitude, ref string geoLongitude)
        {
            var data = url.Substring(url.IndexOf("data=") + 5);
            if (data.IndexOf('?') > 0)
            {
                data = data.Substring(0, data.IndexOf('?'));
            }
            var tokens = data.Split('!');
            var found = false;
            if (tokens.Length > 4)
            {
                foreach (var item in tokens)
                {
                    if (item.StartsWith("3d"))
                    {
                        geoLatitude = item.Substring(2);
                    }
                    if (item.StartsWith("4d"))
                    {
                        geoLongitude = item.Substring(2);
                        found = true;
                    }
                }
            }
            return found;

            // E.G.

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
        }
    }
}
