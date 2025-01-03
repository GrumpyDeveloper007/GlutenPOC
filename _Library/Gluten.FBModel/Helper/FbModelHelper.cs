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

        /// <summary>
        /// Get simplified node from group root
        /// </summary>
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
