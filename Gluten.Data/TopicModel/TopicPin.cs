using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.TopicModel
{
    public class TopicPin
    {
        public string? Label { get; set; }
        public string? Address { get; set; }
        public string? Type { get; set; }//= PinType.Place
        public string? GeoLatitude { get; set; }
        public string? GeoLongitude { get; set; }
    }

    public class TopicPinCache
    {
        public string? PlaceName { get; set; }
        public string? Label { get; set; }
        public string? Address { get; set; }
        public string? GeoLatitude { get; set; }
        public string? GeoLongitude { get; set; }
        public string? MapsUrl { get; set; }
        public string? MetaHtml { get; set; }
        public PinCacheMeta? MetaData { get; set; }
    }

    public class PinCacheMeta
    {
        public string? RestaurantType { get; set; }
        public string? Price { get; set; }
        public string? Stars { get; set; }

        public bool PermanentlyClosed { get; set; }
        // Opening times 
        // review summary
        // website 
        // opening times 
        // phone no
        // address 
        // Dine-in, Takeaway, Delivery?

    }
}
