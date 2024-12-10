// Ignore Spelling: serp iem hcm

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore IDE1006 // Naming Styles
