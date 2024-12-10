// Ignore Spelling: lat lon admin

using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Provides functionality to help identify city
    /// </summary>
    public class CityService
    {
        private const string GeoJsonFilePath = "Resource\\cities500.json";

        private readonly List<City> _cityList;

        public class City
        {
#pragma warning disable IDE1006 // Naming Styles

            public required string id { get; set; }
            public required string name { get; set; }
            public required string country { get; set; } // country code
            public required string admin1 { get; set; }
            public required string lat { get; set; }
            public required string lon { get; set; }
            public required string pop { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        }

        /// <summary>
        /// Loads the database
        /// </summary>
        public CityService()
        {
            var reader = new GeoJsonReader();
            using var stream = new StreamReader(GeoJsonFilePath);
            var geoJson = stream.ReadToEnd();
            _cityList = reader.Read<List<City>>(geoJson);
        }

        /// <summary>
        /// Returns true if the given string is a city name
        /// </summary>
        public bool IsCity(string city)
        {
            List<string> cities = ["Krakow", "Reykjavik", "Cancun", "New York", "Rhodes",
                "Penang", "Tenerife","Frankfurt"];
            if (_cityList.Any(o => o.name == city)) return true;
            if (cities.Any(o => o == city)) return true;
            return false;
        }

        /// <summary>
        /// Returns city class if the given string is a city name
        /// </summary>
        public City? GetCity(string city)
        {
            return _cityList.Where(o => o.name == city).FirstOrDefault<City>();
        }

    }
}
