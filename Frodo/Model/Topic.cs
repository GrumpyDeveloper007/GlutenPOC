using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Model
{
    class Topic
    {
        public string? Title { get; set; }
        public List<string> Responses { get; set; } = new List<string>();

        public List<string>? HashTags { get; set; } = null;

        public List<string>? Urls { get; set; }

        public TopicPin? TopicPin { get; set; }

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
                foreach (var response in Responses)
                    if (response.ToLower().Contains("https://"))
                        return true;
                return false;
            }
        }

        public bool ResponseHasMapLink
        {
            get
            {
                foreach (var response in Responses)
                    if (response.ToLower().Contains("https://maps"))
                        return true;
                return false;
            }
        }
    }
}
