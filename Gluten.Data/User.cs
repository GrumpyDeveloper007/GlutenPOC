using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Name { get; set; }

        public string? DeviceId { get; set; }

    }
}
