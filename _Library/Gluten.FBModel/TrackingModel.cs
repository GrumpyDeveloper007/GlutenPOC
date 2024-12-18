// Ignore Spelling: psn fbtype fbid qid tl mf objid ent attachement actrs tds flgs

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618
#pragma warning disable IDE1006

namespace Gluten.FBModel
{
    public class _379994195544478
    {
        public string page_id { get; set; }
        public string page_id_type { get; set; }
        public long actor_id { get; set; }
        public Dm dm { get; set; }
        public string psn { get; set; }
        public PostContext post_context { get; set; }
        public long role { get; set; }
        public long sl { get; set; }
    }

    public class _100008943645323
    {
        public string page_id { get; set; }
        public string page_id_type { get; set; }
        public string actor_id { get; set; }
        public Dm dm { get; set; }
        public string psn { get; set; }
        public PostContext post_context { get; set; }
        public int role { get; set; }
        public int sl { get; set; }
        public List<Target> targets { get; set; }
    }



    public class Dm
    {
        public long isShare { get; set; }
        public long originalPostOwnerID { get; set; }
        public long sharedMediaID { get; set; }
        public long sharedMediaOwnerID { get; set; }
    }

    public class PageInsights
    {
        [JsonProperty("379994195544478")]
        public _379994195544478 _379994195544478 { get; set; }
        [JsonProperty("361337232353766")]
        public _379994195544478 _361337232353766 { get; set; }

        [JsonProperty("100008943645323")]
        public _100008943645323 _100008943645323 { get; set; }

        [JsonProperty("660915839470807")]
        public _379994195544478 _660915839470807 { get; set; }

        [JsonProperty("61561347076615")]
        public _379994195544478 _61561347076615 { get; set; }

        [JsonProperty("330239776846883")]
        public _379994195544478 _330239776846883 { get; set; }

        [JsonProperty("353439621914938")]
        public _379994195544478 _353439621914938 { get; set; }
        [JsonProperty("319517678837045")]
        public _379994195544478 _319517678837045 { get; set; }
        [JsonProperty("852980778556330")]
        public _379994195544478 _852980778556330 { get; set; }
        [JsonProperty("100083231515067")]
        public _379994195544478 _100083231515067 { get; set; }

        [JsonProperty("1015752345220391")]
        public _379994195544478 _1015752345220391 { get; set; }
        [JsonProperty("422262581142441")]
        public _379994195544478 _422262581142441 { get; set; }
        [JsonProperty("302515126584130")]
        public _379994195544478 _302515126584130 { get; set; }
        [JsonProperty("1420852834795381")]
        public _379994195544478 _1420852834795381 { get; set; }
        [JsonProperty("1053129328213251")]
        public _379994195544478 _1053129328213251 { get; set; }

        [JsonProperty("182984958515029")]
        public _379994195544478 _182984958515029 { get; set; }
        [JsonProperty("3087018218214300")]
        public _379994195544478 _3087018218214300 { get; set; }
        [JsonProperty("769136475365475")]
        public _379994195544478 _769136475365475 { get; set; }
        [JsonProperty("823200180025057")]
        public _379994195544478 _823200180025057 { get; set; }
        [JsonProperty("100058326253164")]
        public _379994195544478 _100058326253164 { get; set; }
        [JsonProperty("1041374946")]
        public _379994195544478 _1041374946 { get; set; }
        [JsonProperty("100075040247767")]
        public _379994195544478 _100075040247767 { get; set; }
        [JsonProperty("422284561238159")]
        public _379994195544478 _422284561238159 { get; set; }
        [JsonProperty("292593134198337")]
        public _379994195544478 _292593134198337 { get; set; }
        [JsonProperty("488425731191722")]
        public _379994195544478 _488425731191722 { get; set; }
        [JsonProperty("687227675922496")]
        public _379994195544478 _687227675922496 { get; set; }
        [JsonProperty("1720098858232675")]
        public _379994195544478 _1720098858232675 { get; set; }
        [JsonProperty("61565218066228")]
        public _379994195544478 _61565218066228 { get; set; }
        [JsonProperty("61555127954574")]
        public _379994195544478 _61555127954574 { get; set; }
        [JsonProperty("450713908359721")]
        public _379994195544478 _450713908359721 { get; set; }
        [JsonProperty("61559586850363")]
        public _379994195544478 _61559586850363 { get; set; }
        [JsonProperty("1573265922")]
        public _379994195544478 _1573265922 { get; set; }
        [JsonProperty("1300758866697297")]
        public _379994195544478 _1300758866697297 { get; set; }
        [JsonProperty("286367932803894")]
        public _379994195544478 _286367932803894 { get; set; }

        [JsonProperty("550373421739534")]
        public _379994195544478 _550373421739534 { get; set; }
        [JsonProperty("383755778784374")]
        public _379994195544478 _383755778784374 { get; set; }
        [JsonProperty("309301445942480")]
        public _379994195544478 _309301445942480 { get; set; }
        [JsonProperty("229495282203436")]
        public _379994195544478 _229495282203436 { get; set; }
        [JsonProperty("247208302148491")]
        public _379994195544478 _247208302148491 { get; set; }
        [JsonProperty("9413340041")]
        public _379994195544478 _9413340041 { get; set; }
        [JsonProperty("61561396146483")]
        public _379994195544478 _61561396146483 { get; set; }
        [JsonProperty("100063657099306")]
        public _379994195544478 _100063657099306 { get; set; }
        [JsonProperty("100001051750172")]
        public _379994195544478 _100001051750172 { get; set; }
        [JsonProperty("625162559593528")]
        public _379994195544478 _625162559593528 { get; set; }
        [JsonProperty("195689771214297")]
        public _379994195544478 _195689771214297 { get; set; }
        [JsonProperty("629876021246440")]
        public _379994195544478 _629876021246440 { get; set; }
        [JsonProperty("847553335358305")]
        public _379994195544478 _847553335358305 { get; set; }
        [JsonProperty("573768437691444")]
        public _379994195544478 _573768437691444 { get; set; }
        [JsonProperty("1452094601717166")]
        public _379994195544478 _1452094601717166 { get; set; }








    }

    public class PostContext
    {
        public long object_fbtype { get; set; }
        public long publish_time { get; set; }
        public string story_name { get; set; }
        public List<string> story_fbid { get; set; }
    }

    public class TrackingRoot
    {
        public string qid { get; set; }
        public string mf_story_key { get; set; }
        public string top_level_post_id { get; set; }
        public string tl_objid { get; set; }
        public long content_owner_id_new { get; set; }
        public List<string> photo_attachments_list { get; set; }
        public string photo_id { get; set; }
        public long story_location { get; set; }
        public string story_attachment_style { get; set; }
        public long sty { get; set; }
        public string ent_attachement_type { get; set; }
        public PageInsights page_insights { get; set; }
        public string actrs { get; set; }
        public long tds_flgs { get; set; }
    }


}
