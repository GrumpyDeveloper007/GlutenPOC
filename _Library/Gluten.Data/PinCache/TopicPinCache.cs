﻿namespace Gluten.Data.PinCache
{
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
}