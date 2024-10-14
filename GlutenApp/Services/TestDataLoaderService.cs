using Maui.GoogleMaps;
using Newtonsoft.Json;

namespace GlutenApp.Services
{
    internal class TestDataLoaderService
    {
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
                var geoLatatude = geo[0][0][0];
                var geoLongitude = geo[0][0][1];
                var geoName = item[11][4][4];
                //Console.WriteLine(geoLatatude + "," + geoLongitude + "," + geoName);

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
