// Ignore Spelling: Facebook

namespace Gluten.Data.ClientModel
{
    /// <summary>
    /// Useful client data for a facebook group post
    /// </summary>
    public class PinLinkInfo
    {
        public string? FacebookUrl { get; set; }
        public string? NodeID { get; set; }
        public string? Title { get; set; }
        public string? ShortTitle { get; set; }
        public DateTimeOffset? PostCreated { get; set; }

    }
}
