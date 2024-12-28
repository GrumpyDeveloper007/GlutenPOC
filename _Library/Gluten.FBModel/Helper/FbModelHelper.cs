// Ignore Spelling: Fb

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gluten.FBModel.Helper
{
    /// <summary>
    /// Provides helper functions to process data returned from FB
    /// </summary>
    public static class FbModelHelper
    {
        //public static IConsole Console { get; set; } = new DummyConsole();

        public static ExtractedModel? GetStoryInfo(Node? storyNode, string nodeId)
        {
            var result = new ExtractedModel();
            if (storyNode == null) return null;
            var story = storyNode.comet_sections.content.story;
            if (story == null) return null;
            var messageText = story?.message?.text;

            if (string.IsNullOrWhiteSpace(messageText))
            {
                Console.WriteLine($"Empty message text, node id = {nodeId}");
                // TODO: Log?
                return null;
            }

            result.Title = messageText;
            result.FacebookUrl = story?.wwwURL ?? "";
            result.GroupId = story?.target_group.id ?? "";
            result.PostCreated = GetTrackingPostDate(story?.story_ufi_container?.story?.tracking ?? "") ?? DateTimeOffset.FromUnixTimeSeconds(0);

            if (result.GroupId == null && story != null)
            {
                result.GroupId = story.target_group.id;
            }
            return result;
        }

        /// <summary>
        /// Gets the topic creation data by decoding the tracking data,
        /// </summary>
        public static DateTimeOffset? GetTrackingPostDate(string trackingInfoString)
        {
            // TODO: Find a better way to support a dynamic group id field name
            long seconds = 0;
            try
            {
                var trackingInfo = JsonConvert.DeserializeObject<TrackingRoot>(trackingInfoString);
                if (trackingInfo == null)
                {
                    Console.WriteLine("Unable to Deserialize tracking info");
                }
                else if (trackingInfo.page_insights._379994195544478 != null)
                    seconds = trackingInfo.page_insights._379994195544478.post_context.publish_time;
                else if (trackingInfo.page_insights._361337232353766 != null)
                    seconds = trackingInfo.page_insights._361337232353766.post_context.publish_time;
                else if (trackingInfo.page_insights._660915839470807 != null)
                    seconds = trackingInfo.page_insights._660915839470807.post_context.publish_time;
                else if (trackingInfo.page_insights._100008943645323 != null)
                    seconds = trackingInfo.page_insights._100008943645323.post_context.publish_time;
                else if (trackingInfo.page_insights._61561347076615 != null)
                    seconds = trackingInfo.page_insights._61561347076615.post_context.publish_time;
                else if (trackingInfo.page_insights._330239776846883 != null)
                    seconds = trackingInfo.page_insights._330239776846883.post_context.publish_time;
                else if (trackingInfo.page_insights._353439621914938 != null)
                    seconds = trackingInfo.page_insights._353439621914938.post_context.publish_time;
                else if (trackingInfo.page_insights._319517678837045 != null)
                    seconds = trackingInfo.page_insights._319517678837045.post_context.publish_time;
                else if (trackingInfo.page_insights._852980778556330 != null)
                    seconds = trackingInfo.page_insights._852980778556330.post_context.publish_time;
                else if (trackingInfo.page_insights._100083231515067 != null)
                    seconds = trackingInfo.page_insights._100083231515067.post_context.publish_time;
                else if (trackingInfo.page_insights._1015752345220391 != null)
                    seconds = trackingInfo.page_insights._1015752345220391.post_context.publish_time;
                else if (trackingInfo.page_insights._422262581142441 != null)
                    seconds = trackingInfo.page_insights._422262581142441.post_context.publish_time;
                else if (trackingInfo.page_insights._302515126584130 != null)
                    seconds = trackingInfo.page_insights._302515126584130.post_context.publish_time;
                else if (trackingInfo.page_insights._1420852834795381 != null)
                    seconds = trackingInfo.page_insights._1420852834795381.post_context.publish_time;
                else if (trackingInfo.page_insights._1053129328213251 != null)
                    seconds = trackingInfo.page_insights._1053129328213251.post_context.publish_time;
                else if (trackingInfo.page_insights._182984958515029 != null)
                    seconds = trackingInfo.page_insights._182984958515029.post_context.publish_time;
                else if (trackingInfo.page_insights._3087018218214300 != null)
                    seconds = trackingInfo.page_insights._3087018218214300.post_context.publish_time;
                else if (trackingInfo.page_insights._769136475365475 != null)
                    seconds = trackingInfo.page_insights._769136475365475.post_context.publish_time;
                else if (trackingInfo.page_insights._823200180025057 != null)
                    seconds = trackingInfo.page_insights._823200180025057.post_context.publish_time;
                else if (trackingInfo.page_insights._100058326253164 != null)
                    seconds = trackingInfo.page_insights._100058326253164.post_context.publish_time;
                else if (trackingInfo.page_insights._1041374946 != null)
                    seconds = trackingInfo.page_insights._1041374946.post_context.publish_time;
                else if (trackingInfo.page_insights._100075040247767 != null)
                    seconds = trackingInfo.page_insights._100075040247767.post_context.publish_time;
                else if (trackingInfo.page_insights._422284561238159 != null)
                    seconds = trackingInfo.page_insights._422284561238159.post_context.publish_time;
                else if (trackingInfo.page_insights._488425731191722 != null)
                    seconds = trackingInfo.page_insights._488425731191722.post_context.publish_time;
                else if (trackingInfo.page_insights._687227675922496 != null)
                    seconds = trackingInfo.page_insights._687227675922496.post_context.publish_time;
                else if (trackingInfo.page_insights._1720098858232675 != null)
                    seconds = trackingInfo.page_insights._1720098858232675.post_context.publish_time;
                else if (trackingInfo.page_insights._61565218066228 != null)
                    seconds = trackingInfo.page_insights._61565218066228.post_context.publish_time;
                else if (trackingInfo.page_insights._61555127954574 != null)
                    seconds = trackingInfo.page_insights._61555127954574.post_context.publish_time;
                else if (trackingInfo.page_insights._450713908359721 != null)
                    seconds = trackingInfo.page_insights._450713908359721.post_context.publish_time;
                else if (trackingInfo.page_insights._1573265922 != null)
                    seconds = trackingInfo.page_insights._1573265922.post_context.publish_time;
                else if (trackingInfo.page_insights._1300758866697297 != null)
                    seconds = trackingInfo.page_insights._1300758866697297.post_context.publish_time;
                else if (trackingInfo.page_insights._286367932803894 != null)
                    seconds = trackingInfo.page_insights._286367932803894.post_context.publish_time;
                else if (trackingInfo.page_insights._61559586850363 != null)
                    seconds = trackingInfo.page_insights._61559586850363?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._292593134198337 != null)
                    seconds = trackingInfo.page_insights._292593134198337?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._550373421739534 != null)
                    seconds = trackingInfo.page_insights._550373421739534?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._383755778784374 != null)
                    seconds = trackingInfo.page_insights._383755778784374?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._309301445942480 != null)
                    seconds = trackingInfo.page_insights._309301445942480?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._229495282203436 != null)
                    seconds = trackingInfo.page_insights._229495282203436?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._247208302148491 != null)
                    seconds = trackingInfo.page_insights._247208302148491?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._9413340041 != null)
                    seconds = trackingInfo.page_insights._9413340041?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._61561396146483 != null)
                    seconds = trackingInfo.page_insights._61561396146483?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063657099306 != null)
                    seconds = trackingInfo.page_insights._100063657099306?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100001051750172 != null)
                    seconds = trackingInfo.page_insights._100001051750172?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._625162559593528 != null)
                    seconds = trackingInfo.page_insights._625162559593528?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._195689771214297 != null)
                    seconds = trackingInfo.page_insights._195689771214297?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._629876021246440 != null)
                    seconds = trackingInfo.page_insights._629876021246440?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._847553335358305 != null)
                    seconds = trackingInfo.page_insights._847553335358305?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._573768437691444 != null)
                    seconds = trackingInfo.page_insights._573768437691444?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._1452094601717166 != null)
                    seconds = trackingInfo.page_insights._1452094601717166?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._1025248344200757 != null)
                    seconds = trackingInfo.page_insights._1025248344200757?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._746699194039212 != null)
                    seconds = trackingInfo.page_insights._746699194039212?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._191520127713279 != null)
                    seconds = trackingInfo.page_insights._191520127713279?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._428227140573301 != null)
                    seconds = trackingInfo.page_insights._428227140573301?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100005382646259 != null)
                    seconds = trackingInfo.page_insights._100005382646259?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100082959441702 != null)
                    seconds = trackingInfo.page_insights._100082959441702?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._1587317368127948 != null)
                    seconds = trackingInfo.page_insights._1587317368127948?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._403103165372802 != null)
                    seconds = trackingInfo.page_insights._403103165372802?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._167644913804569 != null)
                    seconds = trackingInfo.page_insights._167644913804569?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._938709760910143 != null)
                    seconds = trackingInfo.page_insights._938709760910143?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100054658493997 != null)
                    seconds = trackingInfo.page_insights._100054658493997?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._205340443215686 != null)
                    seconds = trackingInfo.page_insights._205340443215686?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._1147493181954443 != null)
                    seconds = trackingInfo.page_insights._1147493181954443?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._61560624580671 != null)
                    seconds = trackingInfo.page_insights._61560624580671?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._110689309272680 != null)
                    seconds = trackingInfo.page_insights._110689309272680?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100083121222818 != null)
                    seconds = trackingInfo.page_insights._100083121222818?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100001999066907 != null)
                    seconds = trackingInfo.page_insights._100001999066907?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063591966209 != null)
                    seconds = trackingInfo.page_insights._100063591966209?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._918867574821402 != null)
                    seconds = trackingInfo.page_insights._918867574821402?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063462763440 != null)
                    seconds = trackingInfo.page_insights._100063462763440?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063752282380 != null)
                    seconds = trackingInfo.page_insights._100063752282380?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._161399357837455 != null)
                    seconds = trackingInfo.page_insights._161399357837455?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063828682053 != null)
                    seconds = trackingInfo.page_insights._100063828682053?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._263520447526095 != null)
                    seconds = trackingInfo.page_insights._263520447526095?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._390608588786276 != null)
                    seconds = trackingInfo.page_insights._390608588786276?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._1235554466913814 != null)
                    seconds = trackingInfo.page_insights._1235554466913814?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100002119293737 != null)
                    seconds = trackingInfo.page_insights._100002119293737?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._503548514144589 != null)
                    seconds = trackingInfo.page_insights._503548514144589?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063520301683 != null)
                    seconds = trackingInfo.page_insights._100063520301683?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063770546152 != null)
                    seconds = trackingInfo.page_insights._100063770546152?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100063587919272 != null)
                    seconds = trackingInfo.page_insights._100063587919272?.post_context?.publish_time ?? 0;
                else if (trackingInfo.page_insights._100062805141501 != null)
                    seconds = trackingInfo.page_insights._100062805141501?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100063618280186 != null)
                    seconds = trackingInfo.page_insights._100063618280186?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100063572320352 != null)
                    seconds = trackingInfo.page_insights._100063572320352?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._497294327007595 != null)
                    seconds = trackingInfo.page_insights._497294327007595?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100063707374581 != null)
                    seconds = trackingInfo.page_insights._100063707374581?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100064362116863 != null)
                    seconds = trackingInfo.page_insights._100064362116863?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100053249987557 != null)
                    seconds = trackingInfo.page_insights._100053249987557?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._105833364200390 != null)
                    seconds = trackingInfo.page_insights._105833364200390?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100075781135940 != null)
                    seconds = trackingInfo.page_insights._100075781135940?.post_context?.publish_time ?? 0;

                else if (trackingInfo.page_insights._100500652379911 != null)
                    seconds = trackingInfo.page_insights._100500652379911?.post_context?.publish_time ?? 0;

                else
                {
                    Console.WriteLine($"Unknown message structure {trackingInfoString}");
                }
                if (seconds > 0)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(seconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Scans nodes looking for 'Story' type
        /// </summary>
        public static Node? GetStoryNode(GroupRoot? groupRoot)
        {
            if (groupRoot == null) return null;
            if (groupRoot == null || groupRoot.data.node == null) return null;

            var node = groupRoot.data.node;

            if (node.__typename == "Group")
            {
                if (node.group_feed.edges.Count > 1)
                {
                    Console.WriteLine("multiple edges");
                }
                node = node.group_feed.edges[0].node;
            }
            else if (node.__typename != "Story")
            {
                Console.WriteLine("unknown node");
            }
            return node;
        }

        public static SimplifiedNode? GetStoryNode(SimplifiedGroupRoot? groupRoot)
        {
            if (groupRoot == null) return null;
            if (groupRoot == null || groupRoot.data.node == null) return null;

            var node = groupRoot.data.node;

            if (node.__typename == "Group")
            {
                if (node.group_feed.edges.Count > 1)
                {
                    Console.WriteLine("multiple edges");
                }
                node = node.group_feed.edges[0].node;
            }
            else if (node.__typename != "Story")
            {
                Console.WriteLine("unknown node");
            }
            return node;
        }


        /// <summary>
        /// Gets Node Ids contained in a search response
        /// </summary>
        public static List<string>? GetNodeIds(SearchRoot? root)
        {
            var results = new List<string>();
            if (root == null || root.data.serpResponse == null) return null;

            foreach (var edge in root.data.serpResponse.results.edges)
            {
                if (edge.relay_rendering_strategy?.__typename == "SearchEndOfResultsModuleRenderingStrategy") continue;
                if (edge?.relay_rendering_strategy?.view_model?.click_model?.story != null)
                {
                    var story = edge.relay_rendering_strategy.view_model.click_model.story;
                    results.Add(story.id);
                }
            }

            return results;
        }

        /// <summary>
        /// Get Node Id contained in a group response
        /// </summary>
        public static string? GetNodeId(SimplifiedGroupRoot? groupRoot)
        {
            if (groupRoot == null)
            {
                Console.WriteLine($"Unknown Node Id, groupRoot = null");
                return null;
            }
            if (groupRoot.data.node == null)
            {
                if (groupRoot.label == null)
                {
                    // todo: extract group info?
                    return null;
                }
                if (groupRoot.data.page_info != null
                    || groupRoot.label.Contains("InstreamVideoAdBreaksPlayer_video")
                    || groupRoot.label.Contains("FBReelsFeedbackBar_feedback")
                    || groupRoot.label.Contains("VideoPlayerWithVideoCardsOverlay_video")
                    || groupRoot.label.Contains("VideoPlayerWithLiveVideoEndscreenAndChaining_video")

                    )
                {
                    return null;
                }
                //"GroupsCometFeedRegularStories_paginationGroup$defer$GroupsCometFeedRegularStories_group_group_feed$page_info"
                //"VideoPlayerRelay_video$defer$InstreamVideoAdBreaksPlayer_video"
                //"CometFeedStoryFBReelsAttachment_story$defer$FBReelsFeedbackBar_feedback"
                //"CometFeedStoryVideoAttachmentVideoPlayer_video$defer$VideoPlayerWithVideoCardsOverlay_video"
                //"CometFeedStoryVideoAttachmentVideoPlayer_video$defer$VideoPlayerWithLiveVideoEndscreenAndChaining_video"
                Console.WriteLine($"Unknown Node Id, groupRoot.data.node = null");
                return null;
            }

            var node = GetStoryNode(groupRoot);
            if (node == null)
            {
                Console.WriteLine($"Unknown Node Id, unable to get story");
                return null;
            }

            //"GroupsCometFeedRegularStories_paginationGroup$stream$GroupsCometFeedRegularStories_group_group_feed"
            return node.id;
        }

    }
}
