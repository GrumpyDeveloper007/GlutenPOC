using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data
{
    /// <summary>
    /// Log of votes for a venue
    /// </summary>
    public class Votes
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VenueId { get; set; }

        public bool IsUpVote { get; set; }

        public string UserName { get; set; }

    }
}
