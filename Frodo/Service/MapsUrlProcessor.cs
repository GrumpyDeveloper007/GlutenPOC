using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Frodo.Model;

namespace Frodo.Service
{
    internal class MapsUrlProcessor
    {
        public bool ProcessMapsUrl(Topic topic, int index)
        {
            if (topic.Urls == null) return true;
            var url = topic.Urls[index];
            HttpService httpService = new HttpService();

            var httpData = httpService.GetAsync(url).Result;
            if (httpData.Contains("CAPTCHA"))
            {
                // ah
                return false;
            }

            if (httpData.Contains("https://www.google.com/maps/preview/place/"))
            {
                var left = httpData.IndexOf("https://www.google.com/maps/preview/place/");
                var right = httpData.IndexOf("\"", left);

                url = httpData.Substring(left, right - left);
                var temp = HttpUtility.UrlDecode(url);
                topic.Urls[index] = temp;
            }
            else if (httpData.Contains("center=")
                && httpData.Contains("https://maps.google.com/maps"))
            {
                // street view response
                //https://maps.google.com/maps/api/staticmap?center=34.6845322%2C135.1840363&
                var temp = HttpUtility.UrlDecode(httpData);
                var left = temp.IndexOf("center=") + "center=".Length;
                var latEnd = temp.IndexOf(",", left);
                var longEnd = temp.IndexOf(",", latEnd + 1);
                var longEnd2 = temp.IndexOf("&", latEnd + 1);
                if (longEnd2 < longEnd)
                {
                    longEnd = longEnd2;
                }

                var lat = temp.Substring(left, latEnd - left);
                var lon = temp.Substring(latEnd + 1, longEnd - latEnd - 1);

                topic.TopicPin = new TopicPin();
                topic.TopicPin.Label = topic.Title;
                topic.TopicPin.Address = topic.Title;
                topic.TopicPin.GeoLatatude = lat;
                topic.TopicPin.GeoLongitude = lon;
            }
            return true;
        }
    }
}
