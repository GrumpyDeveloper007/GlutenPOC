﻿// Ignore Spelling: Geo

using Gluten.Data.ClientModel;
using Gluten.Data.MapsModel;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Provides geo location functionality
    /// </summary>
    public class GeoService
    {
        private readonly NetTopologySuite.Features.FeatureCollection _features;

        // GeoJSON file path
        //string geoJsonFilePath = "Resource\\ne_10m_admin_0_countries.geojson"; //1066
        //string geoJsonFilePath = "Resource\\world-administrative-boundaries.geojson";
        //string geoJsonFilePath = "Resource\\countries.geojson

        // This seems to be the best fitting database obtainable online
        private const string GeoJsonFilePath = "Resource\\World-EEZ.geojson";

        /// <summary>
        /// Constructor - load data
        /// </summary>
        public GeoService()
        {
            // Load GeoJSON
            var reader = new GeoJsonReader();
            using var stream = new StreamReader(GeoJsonFilePath);
            var geoJson = stream.ReadToEnd();
            _features = reader.Read<NetTopologySuite.Features.FeatureCollection>(geoJson);
        }

        /// <summary>
        /// Gets the country name for the given coordinates
        /// </summary>
        public string GetCountryPin(GMapsPin? pin)
        {
            if (pin == null) return "";
            if (pin.GeoLongitude == null || pin.GeoLatitude == null) return "";
            double longitude = double.Parse(pin.GeoLongitude);
            double latitude = double.Parse(pin.GeoLatitude);
            return GetCountry(longitude, latitude);
        }

        /// <summary>
        /// Gets the country name for the given coordinates
        /// </summary>
        public string GetCountryPin(TopicPin? pin)
        {
            if (pin == null) return "";
            if (pin.GeoLongitude == null || pin.GeoLatitude == null) return "";
            double longitude = double.Parse(pin.GeoLongitude);
            double latitude = double.Parse(pin.GeoLatitude);
            return GetCountry(longitude, latitude);
        }

        /// <summary>
        /// Gets the country name for the given coordinates
        /// </summary>
        public string GetCountryPin(PinTopic? pin)
        {
            if (pin == null) return "";
            return GetCountry(pin.GeoLongitude, pin.GeoLatitude);
        }

        /// <summary>
        /// Gets the country name for the given coordinates
        /// </summary>
        public string GetCountryPin(TopicPinCache? pin)
        {
            if (pin == null) return "";
            if (pin.GeoLongitude == null || pin.GeoLatitude == null) return "";
            double longitude = double.Parse(pin.GeoLongitude);
            double latitude = double.Parse(pin.GeoLatitude);
            return GetCountry(longitude, latitude);
        }

        /// <summary>
        /// Gets a list of all countries in the database
        /// </summary>
        public List<string> GetCountries()
        {
            List<string> values = [];
            foreach (var feature in _features)
            {
                if (feature.Attributes != null)
                {
                    var attribute = feature.Attributes["Country"];
                    if (!string.IsNullOrWhiteSpace(attribute.ToString()))
                        values.Add(attribute.ToString() ?? "");
                }
            }
            return values;

        }

        /// <summary>
        /// Gets the country name for the given coordinates
        /// </summary>
        public string GetCountry(double longitude, double latitude)
        {
            // Define the point (latitude, longitude)
            var factory = new GeometryFactory();
            var point = factory.CreatePoint(new Coordinate(longitude, latitude)); // Example: Rome, Italy

            // Find which country contains the point
            foreach (var feature in _features)
            {
                try
                {
                    if (feature.Geometry.Contains(point))
                    {
                        if (feature.Attributes != null)
                        {
                            var attribute = feature.Attributes["Country"];
                            return attribute.ToString() ?? "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return "";
        }

    }
}
