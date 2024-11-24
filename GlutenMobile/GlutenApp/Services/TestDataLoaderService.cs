using Gluten.Data.TopicModel;
using Maui.GoogleMaps;
using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;

namespace GlutenApp.Services
{
    internal class TestDataLoaderService
    {
        public async Task LoadTopicMauiAsset(Maui.GoogleMaps.Map map)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("Topics.json");
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
            var test = JsonConvert.DeserializeObject<List<DetailedTopic>>(contents);

            var pins = new List<Pin>();
            foreach (var item in test)
            {
                foreach (var messageUrl in item.UrlsV2)
                {
                    if (messageUrl.Pin == null) continue;
                    var geoLatatude = double.Parse(messageUrl.Pin.GeoLatitude);
                    var geoLongitude = double.Parse(messageUrl.Pin.GeoLongitude);
                    var geoName = item.Title.Substring(0, 30);
                    //Console.WriteLine(geoLatatude + "," + geoLongitude + "," + geoName);

                    if (!IsPinInList(map, geoLatatude, geoLongitude))
                    {
                        var pin = new Pin
                        {
                            Label = geoName,
                            Address = geoName,
                            Type = PinType.Place,
                            Position = new Position(geoLatatude, geoLongitude),
                        };
                        map.Pins.Add(pin);
                    }
                }

                foreach (var response in item.ResponsesV2)
                {
                    if (response.Links == null) continue;
                    foreach (var messageUrl in response.Links)
                    {

                        if (messageUrl.Pin == null) continue;
                        var geoLatatude = double.Parse(messageUrl.Pin.GeoLatitude);
                        var geoLongitude = double.Parse(messageUrl.Pin.GeoLongitude);
                        var geoName = response.Message.Substring(0, 30);
                        //Console.WriteLine(geoLatatude + "," + geoLongitude + "," + geoName);

                        if (!IsPinInList(map, geoLatatude, geoLongitude))
                        {

                            var pin = new Pin
                            {
                                Label = geoName,
                                Address = geoName,
                                Type = PinType.Place,
                                Position = new Position(geoLatatude, geoLongitude),
                            };
                            map.Pins.Add(pin);
                        }
                    }
                }
            }

        }



        /// <summary>
        /// Loads some test data for the map control
        /// </summary>
        public async Task LoadMauiAsset(Maui.GoogleMaps.Map map)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("bookmarks.json");
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
            dynamic test = JsonConvert.DeserializeObject(contents);
            var mapName = test[0];
            var locations = test[1][0];
            var locations2 = locations[17];

            var pins = new List<Pin>();
            foreach (var item in locations2)
            {
                var geo = item[11][0][6];
                var geoName = item[11][4][4];
                double geoLatatude = 0;
                double geoLongitude = 0;
                Console.WriteLine(geo[0][0][0] + "," + geo[0][0][1] + "," + geoName);
                if (double.TryParse(geo[0][0][0].ToString(), out geoLatatude)
                    && double.TryParse(geo[0][0][1].ToString(), out geoLongitude))
                {


                    if (!IsPinInList(map, geoLatatude, geoLongitude))
                    {
                        var pin = new Pin
                        {
                            Label = geoName,
                            Address = geoName,
                            Type = PinType.Place,
                            Position = new Position(geoLatatude, geoLongitude),
                        };
                        map.Pins.Add(pin);
                    }
                }

            }
        }

        private bool IsPinInList(Maui.GoogleMaps.Map map, double geoLatatude, double geoLongitude)
        {
            var found = false;
            foreach (var pin in map.Pins)
            {
                if (pin.Position.Latitude == geoLatatude &&
                    pin.Position.Longitude == geoLongitude)
                {
                    found = true;
                }
            }
            return found;
        }
    }
}
