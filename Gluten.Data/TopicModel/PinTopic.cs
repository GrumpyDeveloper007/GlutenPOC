using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.TopicModel
{
    public class PinTopic
    {
        public double GeoLongitude { get; set; }
        public double GeoLatatude { get; set; }
        public string? Label { get; set; }
        public string? Description { get; set; }
        public List<PinLinkInfo>? Topics { get; set; }
        public string? MapsLink { get; set; }
    }

    public class PinLinkInfo
    {
        public required string Title { get; set; }
        public string? FacebookUrl { get; set; }
        public string? NodeID { get; set; }
    }
}
