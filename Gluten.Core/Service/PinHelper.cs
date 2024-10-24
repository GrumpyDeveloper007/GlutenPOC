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
    public class PinHelper
    {
        /// <summary>
        /// Tries to extract a map location from the geo fields in the url for the centre of the map then tries to location 
        /// the actual location from the data= section
        /// </summary>
        public TopicPin? TryToGenerateMapPin(string url)
        {
            if (url == null) return null;
            if (url.Contains("/@"))
            {
                var left = url.IndexOf("/@") + 2;
                var latEnd = url.IndexOf(",", left);
                var longEnd = url.IndexOf(",", latEnd + 1);

                if (latEnd > 0)
                {

                    var lat = url.Substring(left, latEnd - left);
                    var lon = url.Substring(latEnd + 1, longEnd - latEnd - 1);

                    TryGetLocationFromDataParameter(url, ref lat, ref lon);

                    var placeStart = url.IndexOf("/place/") + "/place/".Length;
                    var placeEnd = url.IndexOf("/", placeStart);

                    //"https://www.google.com/maps/preview/place/Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyacho,+3%E2%88%9223+onwa/@34.6785478,135.8161308,3281a,13.1y/data\\\\u003d!4m2!3m1!1s0x60013a30562e78d3:0xd712400d34ea1a7b\\"
                    //                                          "Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyac"
                    var placeName = url.Substring(placeStart, placeEnd - placeStart);
                    placeName = HttpUtility.UrlDecode(placeName);

                    return new TopicPin()
                    {
                        Label = placeName,
                        Address = placeName,
                        GeoLatatude = lat,
                        GeoLongitude = lon
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// Extracts the data= part of a google maps url and looks for the longitude and latitude
        /// </summary>
        public void TryGetLocationFromDataParameter(string url, ref string geoLat, ref string geoLong)
        {
            var data = url.Substring(url.IndexOf("data=") + 5);
            data = data.Substring(0, data.IndexOf("?"));
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
                    }
                }

            }
        }
    }
}
