using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Model
{
    class Topic
    {
        public required string Title { get; set; }
        public List<Response> ResponsesV2 { get; set; } = new List<Response>();

        public List<string>? HashTags { get; set; } = null;

        public List<string>? Urls { get; set; }

        public TopicPin? TopicPin { get; set; }

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

        public bool HasLink
        {
            get
            {
                if (Title == null) return false;
                return Title.ToLower().Contains("https://");
            }
        }

        public bool HasMapLink
        {
            get
            {
                if (Title == null) return false;
                return Title.ToLower().Contains("https://maps");
            }
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
