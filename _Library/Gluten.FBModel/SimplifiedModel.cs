using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.FBModel
{
    internal class SimplifiedModel
    {
    }

    public class SimplifiedGroupRoot
    {
        public string label { get; set; }
        public SimplifiedData data { get; set; }
    }

    public class SimplifiedData
    {
        public SimplifiedNode node { get; set; }
        //public string cursor { get; set; }
        public PageInfo? page_info { get; set; }
        //public Group group { get; set; }
    }

    public class SimplifiedGroupFeed
    {
        public List<SimplifiedEdge> edges { get; set; }
    }

    public class SimplifiedEdge
    {
        //public bool visible_in_bling_bar { get; set; }
        public SimplifiedNode node { get; set; }
        //public string i18n_reaction_count { get; set; }
        //public int reaction_count { get; set; }
        //public string cursor { get; set; }

        //public List<object> debug_overlay_info { get; set; }
        //public string logging_unit_id { get; set; }
        //public bool has_relay_child_rendering_strategy { get; set; }
        //public RelayRenderingStrategy relay_rendering_strategy { get; set; }
    }

    public class SimplifiedNode
    {
        //public string role { get; set; }

        public SimplifiedGroupFeed group_feed { get; set; }

        public string __typename { get; set; }
        //public string __isFeedUnit { get; set; }
        //public string __isCacheable { get; set; }
        //public string cache_id { get; set; }
        //public object debug_info { get; set; }
        public string id { get; set; }
        //public object sponsored_data { get; set; }
        //public Feedback feedback { get; set; }
        //public object is_story_civic { get; set; }
        //public List<object> matched_terms { get; set; }
        //public string post_id { get; set; }
        //public object cix_screen { get; set; }
        //public FutureOfFeedInfo future_of_feed_info { get; set; }
        //public object attached_story { get; set; }
        //public object bumpers { get; set; }
        // TODO:
        //public CometSections comet_sections { get; set; }
        //public string encrypted_tracking { get; set; }
        //public bool should_host_actor_link_in_watch { get; set; }
        //public object whatsapp_ad_context { get; set; }
        //public object schema_context { get; set; }
        //public string click_tracking_linkshim_cb { get; set; }
        //public string encrypted_click_tracking { get; set; }
        //public ModuleOperationCometFeedUnitContainerSectionFeedUnit __module_operation_CometFeedUnitContainerSection_feedUnit { get; set; }
        //public ModuleComponentCometFeedUnitContainerSectionFeedUnit __module_component_CometFeedUnitContainerSection_feedUnit { get; set; }
        //public string __isTrackableFeedUnit { get; set; }
        //public Trackingdata trackingdata { get; set; }
        //public List<int> viewability_config { get; set; }
        //public ClientViewConfig client_view_config { get; set; }
        //public string __isNode { get; set; }
        //public string localized_name { get; set; }

        //public Title title { get; set; }
        //public TargetGroup target_group { get; set; }
        //public StoryHeader story_header { get; set; }
    }

}
