using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data
{
    internal class FoodType
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VenueId { get; set; }

        public string? Name { get; set; }


    }
}
