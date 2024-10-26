using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.TopicModel
{
    public class Topic
    {
        public required string Title { get; set; }
        public List<Response> ResponsesV2 { get; set; } = new List<Response>();

        public List<string>? HashTags { get; set; } = null;

        public List<TopicLink>? UrlsV2 { get; set; }

        public List<AiInformation>? AiTitleInfoV2 { get; set; }

        public bool AiParsed { get; set; } = false;

        public string? FacebookUrl { get; set; }

        public List<AiVenue>? AiVenues { get; set; }

        public bool AiIsQuestion { get; set; }

        public bool AiHasRestaurants { get; set; }

        public bool HasMapPin()
        {
            if (UrlsV2 == null) return false;
            foreach (var url in UrlsV2)
            {
                if (url.Pin != null) return true;
            }
            return false;
        }

        public string? NodeID { get; set; }

        public string GetHashTags()
        {
            string tags = "";
            if (HashTags == null) return tags;
            foreach (var tag in HashTags)
            {
                tags += tag + ",";
            }
            return tags;
        }

        public bool HasLink()
        {
            if (Title == null) return false;
            return Title.ToLower().Contains("https://");
        }

        public bool HasMapLink()
        {
            if (Title == null) return false;
            return Title.ToLower().Contains("https://maps");
        }

        public bool ResponseHasLink
        {
            get
            {
                foreach (var response in ResponsesV2)
                {
                    if (response.Message == null) return false;
                    if (response.Message.ToLower().Contains("https://"))
                        return true;
                }
                return false;
            }
        }

        public bool ResponseHasMapLink
        {
            get
            {
                foreach (var response in ResponsesV2)
                {
                    if (response.Message == null) return false;
                    if (response.Message.ToLower().Contains("https://maps"))
                        return true;
                }
                return false;
            }
        }
    }
}
