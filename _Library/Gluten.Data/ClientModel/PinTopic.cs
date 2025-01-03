using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.ClientModel
{
    /// <summary>
    /// Data structure used in the client application, represents a pin on the map
    /// </summary>
    public class PinTopic
    {
        public double GeoLongitude { get; set; }
        public double GeoLatitude { get; set; }
        public string Label { get; set; } = "";
        public string? Description { get; set; }
        public List<PinLinkInfo> Topics { get; set; } = [];
        public string? MapsLink { get; set; }
        public string? RestaurantType { get; set; }
        public string? Price { get; set; }
        public string? Stars { get; set; }
    }
}
