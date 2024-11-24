using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.TopicModel
{
    public class PinDescriptionCache
    {
        public List<string> Nodes { get; set; } = new List<string>();
        public string? Description { get; set; }
    }
}
