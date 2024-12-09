using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.FBModel
{

    public class SearchRoot
    {
        public SearchData data { get; set; }
        public Extensions extensions { get; set; }
    }
    public class SearchData
    {
        public SerpResponse serpResponse { get; set; }
    }

    public class SerpResponse
    {
        public Results results { get; set; }
    }

    public class Results
    {
        public List<Edge> edges { get; set; }
        public bool has_iem_triggered { get; set; }
        public bool has_hcm { get; set; }
        public string logging_unit_id { get; set; }
        public PageInfo page_info { get; set; }
    }

}
