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
        public string? GeoLatatude { get; set; }
        public string? GeoLongitude { get; set; }
        public string? MapsUrl { get; set; }

        public string? MetaHtml { get; set; }
    }
}
