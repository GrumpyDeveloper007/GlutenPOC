using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data
{
    public class MenuItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VenueId { get; set; }

        public string? Name { get; set; }

        public bool IsGlutenFree { get; set; }

    }
}
