// Ignore Spelling: Facebook

namespace Gluten.Data.TopicModel
{
    public class DetailedTopic
    {
        public string? TitleCountry { get; set; }

        public string? TitleCity { get; set; }
        public required string Title { get; set; }
        public string? ShortTitle { get; set; }
        public bool ShortTitleProcessed { get; set; } = false;

        public string? FacebookUrl { get; set; }
        public string? NodeID { get; set; }

        public List<AiVenue>? AiVenues { get; set; }

        public List<TopicLink>? UrlsV2 { get; set; }

        public List<Response> ResponsesV2 { get; set; } = [];

        public string GroupId { get; set; } = "";
        public DateTimeOffset? PostCreated { get; set; }

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

        public bool HasLink()
        {
            if (Title == null) return false;
            return Title.Contains("https://", StringComparison.CurrentCultureIgnoreCase);
        }

        public bool HasMapLink()
        {
            if (Title == null) return false;
            return Title.Contains("https://maps", StringComparison.CurrentCultureIgnoreCase);
        }

        public bool ResponseHasLink
        {
            get
            {
                foreach (var response in ResponsesV2)
                {
                    if (response.Message == null) return false;
                    if (response.Message.Contains("https://", StringComparison.CurrentCultureIgnoreCase))
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
                    if (response.Message.Contains("https://maps", StringComparison.CurrentCultureIgnoreCase))
                        return true;
                }
                return false;
            }
        }
    }
}
