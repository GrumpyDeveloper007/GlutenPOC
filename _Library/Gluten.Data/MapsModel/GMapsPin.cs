﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.MapsModel
{
    public class GMapsPin
    {
        public string? PlaceName { get; set; }
        public string? Label { get; set; }
        public string? Address { get; set; }
        public string? GeoLatitude { get; set; }
        public string? GeoLongitude { get; set; }
        public string? MapsUrl { get; set; }
        public string? RestaurantType { get; set; }
        public string? Price { get; set; }
        public string? Stars { get; set; }
        public string? Comment { get; set; }

    }
}