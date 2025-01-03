using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Data.TopicModel;
using Gluten.FBModel;
using Gluten.FBModel.Helper;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Frodo.Service
{
    /// <summary>
    /// Processes the data captured from FB groups
    /// </summary>
    internal class TopicLoaderService(TopicsDataLoaderService _topicsLoaderService, FBGroupService _fBGroupService, IConsole Console)
    {
        internal static readonly string[] crlf = ["/r/n"];

        /// <summary>
        /// Loads the data objects captured from FB groups, extracts the information we are interested in
        /// </summary>
        public void ReadFileLineByLine(string filePath, List<DetailedTopic> topics)
        {
            if (!File.Exists(filePath)) return;
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
            _topicsLoaderService.SaveTopics(topics);

        }

        private void ProcessSearchRootMessage(string message, List<DetailedTopic> topics)
        {
            var sr = JsonConvert.DeserializeObject<SearchRoot>(message);
            if (sr?.data?.serpResponse == null) return;

            foreach (var edge in sr.data.serpResponse.results.edges)
            {
                if (edge.relay_rendering_strategy?.__typename == "SearchEndOfResultsModuleRenderingStrategy") continue;
                if (edge.relay_rendering_strategy?.view_model?.click_model?.story == null) continue;

                var story = edge.relay_rendering_strategy.view_model.click_model.story;
                var comet_sectionsStory = story.comet_sections.content.story;
                var metaData = story.comet_sections.context_layout.story.comet_sections.metadata;

                //TODO: Assume element 0, CometFeedStoryLongerTimestampStrategy
                var metaStoryCreationTime = metaData[0].story.creation_time;
                var nodeId = story.id;
                var messageText = comet_sectionsStory?.message?.text;
                // If this is a linked story, ignore
                if (string.IsNullOrWhiteSpace(messageText)) continue;
                DetailedTopic? currentTopic = TopicHelper.GetOrCreateTopic(topics, nodeId, messageText, out var isExisting);

                currentTopic.Title = messageText ?? "";
                currentTopic.FacebookUrl = comet_sectionsStory?.wwwURL;
                currentTopic.PostCreated = DateTimeOffset.FromUnixTimeSeconds(metaStoryCreationTime);

                if (string.IsNullOrWhiteSpace(currentTopic.GroupId))
                {
                    currentTopic.GroupId = comet_sectionsStory?.target_group.id ?? "";
                }
                if (!isExisting) topics.Add(currentTopic);
            }
        }


        private void ProcessModel(GroupRoot? groupRoot, List<DetailedTopic> topics)
        {
            if (groupRoot == null) return;
            if (groupRoot == null || groupRoot.data.node == null) return;

            var stories = FbStoryModelHelper.FlattenNodes(groupRoot?.data?.node);
            stories = FbStoryModelHelper.CombineStories(stories);

            var node = FbModelHelper.GetStoryNode(groupRoot);
            if (node == null) return;
            var nodeId = node.id;

            DetailedTopic? currentTopic = TopicHelper.GetOrCreateTopic(topics, nodeId, "", out var isExisting);
            UpdateTopicFromStoryNodeV2(node, currentTopic, stories);
            if (!_fBGroupService.IsFilteredGroup(currentTopic.GroupId))
            {
                if (!isExisting) topics.Add(currentTopic);
            }
        }

        private void UpdateTopicFromStoryNodeV2(Node node, DetailedTopic currentTopic, List<Story> stories)
        {
            var story = stories.SingleOrDefault(o => o.id == node.id);
            if (story == null)
            {
                Console.WriteLineRed($"Error finding story id :{node.id}");
                return;
            }

            var messageText = story.message?.text;

            if (currentTopic.Title != messageText
                && !string.IsNullOrWhiteSpace(messageText)
                && !string.IsNullOrWhiteSpace(currentTopic.Title))
            {
                Console.WriteLineBlue($"Change detection {currentTopic.Title} new : {messageText}");
            }
            if (currentTopic.FacebookUrl != story.wwwURL
                && !string.IsNullOrWhiteSpace(currentTopic.FacebookUrl))
            {
                Console.WriteLineBlue($"Change detection {currentTopic.FacebookUrl} new : {story.wwwURL}");
            }
            if (currentTopic.PostCreated != DateTimeOffset.FromUnixTimeSeconds(story.creation_time))
            {
                //Console.WriteLineBlue($"Change detection {currentTopic.PostCreated} new : {DateTimeOffset.FromUnixTimeSeconds(story.creation_time)}");
            }
            var groupId = story?.target_group?.id;
            groupId ??= story?.comet_sections?.action_link?.group.id;

            if (story?.attached_story?.comet_sections?.message?.story != null)
            {
                var ignoreList = new List<string> {
                    "Gluten Free Global",
                    "glutenfreerecipes",
                    "Instructions:",
                    "INGREDIENTS",
                    "veganrecipe"
                    };

                var linkedStory = stories.SingleOrDefault(o => o.id == story?.attached_story?.comet_sections?.message?.story.id);
                if (linkedStory != null
                    && !ignoreList.Exists(o => linkedStory.message.text.Contains(o))
                    )
                {
                    // Append linked story text
                    Console.WriteLineBlue($"{story.wwwURL}");
                    //Console.WriteLineBlue($"Adding linked story text :From {linkedStory.actors[0].name} - {linkedStory.message.text}");
                    messageText += $" , From {linkedStory.actors[0].name} - {linkedStory.message.text}";
                }
            }

            var interesting_top_level_comments = story?.feedback_context?.interesting_top_level_comments;
            if (interesting_top_level_comments != null)
            {
                foreach (var feedback in interesting_top_level_comments)
                {
                    var d = feedback.comment;
                    if (d.body != null)
                    {
                        Response currentResponse = TopicHelper.GetOrCreateResponse(currentTopic, feedback.comment.id);
                        currentResponse.Message = d.body.text;
                    }
                }
            }

            if (currentTopic.Title != messageText
                || currentTopic.GroupId != story?.target_group.id)
            {
                // Text updated, refresh data
                currentTopic.CitySearchDone = false;
                currentTopic.IsAiVenuesSearchDone = false;
                currentTopic.ShortTitleProcessed = false;
            }
            currentTopic.Title = messageText ?? "";
            currentTopic.FacebookUrl = story?.wwwURL;
            currentTopic.PostCreated = DateTimeOffset.FromUnixTimeSeconds(story?.creation_time ?? 0);
            currentTopic.GroupId = groupId ?? "";
        }
    }
}
