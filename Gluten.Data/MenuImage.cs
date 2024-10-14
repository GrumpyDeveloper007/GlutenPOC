using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data
{
    /// <summary>
    /// Data store for menu images
    /// </summary>
    public class MenuImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VenueId { get; set; }


        /// <summary>
        /// Name of the menu image
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the menu image
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// File name of the menu image
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The last time this menu image was modified
        /// </summary>
        public DateTimeOffset LastModificationDateTime { get; set; }

    }
}
