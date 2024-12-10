using Gluten.Core.Interface;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Gluten.FBModel;
using Gluten.FBModel.Helper;
using Newtonsoft.Json;

namespace Frodo.Service
{
    /// <summary>
    /// Processes the data captured from FB groups
    /// </summary>
    internal class TopicLoaderService(IConsole Console)
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

                    if (line.StartsWith("{\"data\":"))
                    {
                        ProcessSearchRootMessage(line, topics);
                    }
                    else
                    {
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
        }

        private void ProcessSearchRootMessage(string message, List<DetailedTopic> topics)
        {
            var sr = JsonConvert.DeserializeObject<SearchRoot>(message);
            if (sr?.data?.serpResponse == null) return;

            foreach (var edge in sr.data.serpResponse.results.edges)
            {
                if (edge.relay_rendering_strategy.__typename == "SearchEndOfResultsModuleRenderingStrategy") continue;
                if (edge.relay_rendering_strategy.view_model.click_model.story == null) continue;

                var story = edge.relay_rendering_strategy.view_model.click_model.story;
                var comet_sectionsStory = story.comet_sections.content.story;
                var metaData = story.comet_sections.context_layout.story.comet_sections.metadata;

                //TODO: Assume element 0, CometFeedStoryLongerTimestampStrategy
                var metaStoryCreationTime = metaData[0].story.creation_time;
                var nodeId = story.id;
                var messageText = comet_sectionsStory?.message?.text;
                // If this is a linked story, ignore
                if (string.IsNullOrWhiteSpace(messageText)) continue;
                DetailedTopic? currentTopic = _topicHelper.GetOrCreateTopic(topics, nodeId, messageText);

                currentTopic.Title = messageText ?? "";
                currentTopic.FacebookUrl = comet_sectionsStory?.wwwURL;
                currentTopic.PostCreated = DateTimeOffset.FromUnixTimeSeconds(metaStoryCreationTime);

                if (string.IsNullOrWhiteSpace(currentTopic.GroupId))
                {
                    currentTopic.GroupId = comet_sectionsStory?.target_group.id ?? "";
                }
            }
        }

        private void ProcessModel(GroupRoot? groupRoot, List<DetailedTopic> topics)
        {
            if (groupRoot == null) return;
            string? messageText;
            if (groupRoot == null || groupRoot.data.node == null) return;

            var node = FbModelHelper.GetStoryNode(groupRoot);
            if (node == null) return;

            var nodeId = node.id;
            var contentStory = node.comet_sections.content.story;

            if (contentStory != null)
            {
                messageText = contentStory?.message?.text;

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    var attachedMessage = contentStory?.attached_story?.message?.text;
                    if (string.IsNullOrWhiteSpace(attachedMessage))
                    {
                        Console.WriteLine($"Empty message text, node id = {nodeId}");
                    }
                    return;
                }
                DetailedTopic? currentTopic = _topicHelper.GetOrCreateTopic(topics, nodeId, messageText);

                UpdateTopicFromStoryNode(node, currentTopic);
            }
        }

        private void UpdateTopicFromStoryNode(Node node, DetailedTopic currentTopic)
        {
            var contentStory = node.comet_sections.content.story;
            var messageText = contentStory?.message?.text;
            var ufiStory = node.comet_sections.feedback.story.story_ufi_container.story;
            var interesting_top_level_comments = ufiStory.feedback_context.interesting_top_level_comments;
            var tracking = ufiStory.tracking;
            foreach (var feedback in interesting_top_level_comments)
            {
                var d = feedback.comment;
                if (d.body != null)
                {
                    Response currentResponse = _topicHelper.GetOrCreateResponse(currentTopic, feedback.comment.id);
                    currentResponse.Message = d.body.text;
                }
            }
            currentTopic.Title = messageText ?? "";
            currentTopic.FacebookUrl = contentStory?.wwwURL;
            currentTopic.PostCreated = FbModelHelper.GetTrackingPostDate(tracking);

            if (string.IsNullOrWhiteSpace(currentTopic.GroupId) && ufiStory?.target_group != null)
            {
                currentTopic.GroupId = ufiStory.target_group.id;
            }

            if (string.IsNullOrWhiteSpace(currentTopic.GroupId) && contentStory?.target_group != null)
            {
                currentTopic.GroupId = contentStory.target_group.id;
            }
        }

    }
}
