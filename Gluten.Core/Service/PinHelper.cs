using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Gluten.Core.Service
{
    public class PinHelper
    {
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
    }
}
