using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Gluten.FBModel;
using Newtonsoft.Json;

namespace Frodo.Service
{
    /// <summary>
    /// Processes the data captured from FB groups
    /// </summary>
    internal class TopicLoaderService
    {
        private readonly TopicHelper _topicHelper = new();
        internal static readonly string[] crlf = ["/r/n"];

        /// <summary>
        /// Loads the data objects captured from FB groups, extracts the information we are interested in
        /// </summary>
        public void ReadFileLineByLine(string filePath, List<DetailedTopic> topics)
        {
            // Open the file and read each line
            using StreamReader sr = new(filePath);
            string? line;
            int i = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line != null)
                {
                    var messages = line.Split(crlf, StringSplitOptions.None);
                    // Process the line
                    i++;
                    if (i % 100 == 0)
                        Console.WriteLine(i);
                    foreach (var message in messages)
                    {
                        try
                        {
                            GroupRoot? m;
                            m = JsonConvert.DeserializeObject<GroupRoot>(message);
                            ProcessModel(m, topics);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        private void ProcessModel(GroupRoot? groupRoot, List<DetailedTopic> topics)
        {
            if (groupRoot == null) return;
            string? messageText;
            if (groupRoot == null || groupRoot.data.node == null) return;

            var a = groupRoot.data.node.comet_sections;
            var nodeId = groupRoot.data.node.id;

            if (groupRoot.data.node.group_feed?.edges != null && a == null)
                foreach (var edge in groupRoot.data.node.group_feed.edges)
                {
                    var node = edge.node;
                    nodeId = node.id;
                    a = node.comet_sections;

                }

            if (a != null)
            {
                var story = a.content.story;
                messageText = story?.message?.text;

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    // TODO: Log?
                    return;
                }
                DetailedTopic? currentTopic = _topicHelper.GetOrCreateTopic(topics, nodeId, messageText);

                var story2 = a.feedback.story.story_ufi_container.story.feedback_context.interesting_top_level_comments;
                foreach (var feedback in story2)
                {
                    var d = feedback.comment;
                    if (d.body != null)
                    {
                        Response currentResponse = _topicHelper.GetOrCreateResponse(currentTopic, feedback.comment.id);
                        currentResponse.Message = d.body.text;
                    }
                }
                currentTopic.Title = messageText;
                currentTopic.FacebookUrl = story?.wwwURL;

                currentTopic.GroupId = a.feedback.story.story_ufi_container.story.target_group.id;
                var b = a.feedback.story.story_ufi_container.story.tracking;
                try
                {
                    var trackingInfo = JsonConvert.DeserializeObject<TrackingRoot>(b);
                    long seconds = 0;
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
                    //1189303823

                    else
                    {
                        Console.WriteLine($"Unknown message structure {b}");
                    }
                    currentTopic.PostCreated = DateTimeOffset.FromUnixTimeSeconds(seconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (currentTopic.GroupId == null && story != null)
                {
                    currentTopic.GroupId = story.target_group.id;
                }
            }

        }

    }
}
