namespace Gluten.Data
{
    /// <summary>
    /// Contains information about a venue
    /// </summary>
    public class Venue
    {
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Name of the venue
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Location of the venue
        /// </summary>
        public double GeoLatitude { get; set; }

        /// <summary>
        /// Location of the venue
        /// </summary>
        public double GeoLongitude { get; set; }

        /// <summary>
        /// A collection of menu images 
        /// </summary>
        public List<string> MenuImageNames { get; set; } = new List<string>();

        /// <summary>
        /// Indicates the last time this information was confirmed by someone
        /// </summary>
        public DateTimeOffset LastVisitDateTime { get; set; }

        /// <summary>
        /// Generic notes field
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Indicates if the venue will modify dishes to make gluten free
        /// </summary>
        public bool CanCustomizeOrder { get; set; }

        public int UpVotes { get; set; }

        public int DownVotes { get; set; }

        public string? FoodType { get; set; }

    }
}
