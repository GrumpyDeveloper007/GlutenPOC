using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.TopicModel
{
    /// <summary>
    /// Represents the fileds needed by the client app
    /// </summary>
    public class Topic
    {
        public required string Title { get; set; }
        public string? ShortTitle { get; set; }

        public string? FacebookUrl { get; set; }
        public string? NodeID { get; set; }

        public List<AiVenue>? AiVenues { get; set; }

        public List<TopicLink>? UrlsV2 { get; set; }
    }
}
