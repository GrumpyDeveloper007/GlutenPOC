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
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
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
