using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Service
{
    public class GeoService
    {
        private NetTopologySuite.Features.FeatureCollection _features;

        // GeoJSON file path
        //string geoJsonFilePath = "Resource\\ne_10m_admin_0_countries.geojson"; //1066
        //string geoJsonFilePath = "Resource\\world-administrative-boundaries.geojson";
        //string geoJsonFilePath = "Resource\\countries.geojson";
        string geoJsonFilePath = "Resource\\World-EEZ.geojson";




        public GeoService()
        {
            // Load GeoJSON
            var reader = new GeoJsonReader();
            using var stream = new StreamReader(geoJsonFilePath);
            var geoJson = stream.ReadToEnd();
            _features = reader.Read<NetTopologySuite.Features.FeatureCollection>(geoJson);
        }

        public string GetCountryPin(TopicPinCache? pin)
        {
            if (pin == null) return "";
            double longitude = double.Parse(pin.GeoLongitude);
            double latitude = double.Parse(pin.GeoLatitude);
            return GetCounty(longitude, latitude);
        }

        //longitude: 12.4924, latitude: 41.8902
        public string GetCounty(double longitude, double latitude)
        {
            // Define the point (latitude, longitude)
            var factory = new GeometryFactory();
            var point = factory.CreatePoint(new Coordinate(longitude, latitude)); // Example: Rome, Italy

            // Find which country contains the point
            foreach (var feature in _features)
            {
                //try
                {
                    if (feature.Geometry.Contains(point))
                    {
                        //return feature.Attributes["ADMIN"].ToString();
                        //return feature.Attributes["name"].ToString();
                        return feature.Attributes["Country"].ToString();
                    }
                }
                //catch (Exception ex)
                {

                }
            }
            //Console.WriteLine($" {latitude}, {longitude}");
            return "";
        }

    }
}
