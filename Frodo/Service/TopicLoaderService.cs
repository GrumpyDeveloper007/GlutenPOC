using Frodo.FacebookModel;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;

namespace Frodo.Service
{
    internal class TopicLoaderService
    {
        private readonly TopicHelper _topicHelper = new();

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
                    var messages = line.Split(new string[] { "/r/n" }, StringSplitOptions.None);
                    // Process the line
                    i++;
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
                if (currentTopic.GroupId == null && story != null)
                {
                    currentTopic.GroupId = story.target_group.id;
                }
            }

        }

    }
}
