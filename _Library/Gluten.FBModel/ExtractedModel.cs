using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.FBModel
{
    public class ExtractedModel
    {
        public string Title { get; set; }
        public string FacebookUrl { get; set; }
        public string GroupId { get; set; }
        public DateTimeOffset PostCreated { get; set; }
    }
}
