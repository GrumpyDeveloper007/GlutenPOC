using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samwise.Service
{
    internal class ParsingUtilities
    {
        private TopicsHelper _topicsHelper = new TopicsHelper();
        private PinHelper _pinHelper = new PinHelper();

        private void FixIncorrectPins(List<Topic> topics, string dbFileName)
        {
            //https://www.google.com/maps/place/Otsuna+Sushi/@35.7281855,139.7452636,17z/data=!4m6!3m5!1s0x60188dbbed184005:0x88ffa854362ddcfd!8m2!3d35.7278459!4d139.7459932!16s%2Fg%2F1tsbm3f7!5m1!1e4?entry=ttu&g_ep=EgoyMDI0MTAyMS4xIKXMDSoASAFQAw%3D%3D
            var incorrectPins = "";
            for (int i = 0; i < topics.Count; i++)
            {
                for (int t = 0; t < topics[i].UrlsV2.Count; t++)
                {
                    if (topics[i].UrlsV2[t].Pin != null)
                    {
                        var geolat = "";
                        var geolong = "";
                        _pinHelper.TryGetLocationFromDataParameter(topics[i].UrlsV2[t].Url, ref geolat, ref geolong);

                        if (geolong == "" || geolong == "") continue;
                        if (topics[i].UrlsV2[t].Pin.GeoLatatude != geolat
                        || topics[i].UrlsV2[t].Pin.GeoLongitude != geolong)
                        {
                            incorrectPins += $"{topics[i].UrlsV2[t].Pin.Label}\r\n";
                            topics[i].UrlsV2[t].Pin.GeoLatatude = geolat;
                            topics[i].UrlsV2[t].Pin.GeoLongitude = geolong;
                        }
                    }
                }
            }
            _topicsHelper.SaveTopics(dbFileName, topics);
        }

    }
}
