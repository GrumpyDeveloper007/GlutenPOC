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

            var node = FbModelHelper.GetStoryNode(groupRoot);
            if (node == null) return;

            var nodeId = node.id;
            var story = node.comet_sections.content.story;
            var attachedMessage = story.attached_story?.message?.text;

            if (story != null)
            {
                messageText = story?.message?.text;

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    if (string.IsNullOrWhiteSpace(attachedMessage))
                    {
                        Console.WriteLine($"Empty message text, node id = {nodeId}");
                    }
                    // TODO: Log?
                    return;
                }
                DetailedTopic? currentTopic = _topicHelper.GetOrCreateTopic(topics, nodeId, messageText);

                var a = node.comet_sections;
                var ufiStory = a.feedback.story.story_ufi_container.story;
                var story2 = a.feedback.story.story_ufi_container.story.feedback_context.interesting_top_level_comments;
                var tracking = a.feedback.story.story_ufi_container.story.tracking;
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
                currentTopic.GroupId = ufiStory.target_group.id;
                currentTopic.PostCreated = FbModelHelper.GetTrackingPostDate(tracking);

                if (currentTopic.GroupId == null && story != null)
                {
                    currentTopic.GroupId = story.target_group.id;
                }
            }

        }

    }
}
