using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.PinDescription
{
    public class PinDescriptionCache
    {
        public List<string> Nodes { get; set; } = [];
        public string? Description { get; set; }
    }
}
