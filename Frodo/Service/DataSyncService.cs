using Azure.AI.TextAnalytics;
using Frodo.FacebookModel;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;

namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// </summary>
    internal class DataSyncService
    {
        private TopicHelper _topicHelper = new TopicHelper();
        private TopicsHelper _topicsHelper = new TopicsHelper();
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor;
        private PinHelper _pinHelper = new PinHelper();
        private AIProcessingService _aIProcessingService;

        public List<Topic> Topics = new List<Topic>();
        private string DBFileName = "D:\\Coding\\Gluten\\Topics.json";

        public DataSyncService(AIProcessingService aIProcessingService,
            SeleniumMapsUrlProcessor seleniumMapsUrlProcessor)
        {
            _aIProcessingService = aIProcessingService;
            _seleniumMapsUrlProcessor = seleniumMapsUrlProcessor;
        }


        public void ProcessFile()
        {
            var topics = _topicsHelper.TryLoadTopics(DBFileName);
            if (topics != null)
            {
                Topics = topics;
            }

            ReadFileLineByLine("D:\\Coding\\Gluten\\Smeagol\\bin\\Debug\\net8.0\\Responses.txt");

            int linkCount = 0;
            int mapsLinkCount = 0;
            int responseLinkCount = 0;
            int responseMapsLinkCount = 0;
            int mapsCallCount = 0;
            string unProcessedUrls = "";
            int unProcessedUrlsCount = 0;
            int pinCount = 0;
            bool tryProcessLinks = true;
            for (int i = 0; i < Topics.Count; i++)
            {
                var topic = Topics[i];
                string responseText = "";
                foreach (var response in topic.ResponsesV2)
                {
                    responseText += response + " , ";
                }
                // Update url list from title
                topic.HashTags = StringHelper.ExtractHashtags(topic.Title);
                var newUrls = StringHelper.ExtractUrls(topic.Title);
                if (topic.UrlsV2 == null)
                {
                    topic.UrlsV2 = newUrls;
                }
                else
                {
                    CheckForUpdatedUrls(topic, newUrls);
                }

                // Check for links in topics
                if (topic.HasLink()) linkCount++;
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    var url = topic.UrlsV2[t].Url;

                    if (topic.UrlsV2[t].Pin == null
                        && tryProcessLinks)
                    {
                        if (topic.UrlsV2[t].Pin == null)
                        {
                            var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(url);
                            topic.UrlsV2[t].Url = newUrl;
                            topic.UrlsV2[t].Pin = _pinHelper.TryToGenerateMapPin(newUrl);
                            mapsCallCount++;
                        }

                    }
                    mapsLinkCount++;
                }

                // Look for links in the responses
                for (int z = 0; z < topic.ResponsesV2.Count; z++)
                {
                    if (topic.ResponsesV2[z].Message == null) continue;

                    var newLinks = StringHelper.ExtractUrls(topic.ResponsesV2[z].Message);
                    if (topic.ResponsesV2[z].Links == null)
                    {
                        topic.ResponsesV2[z].Links = newLinks;
                    }

                    var links = topic.ResponsesV2[z].Links;
                    if (links != null)
                    {
                        for (int t = 0; t < links.Count; t++)
                        {
                            var url = links[t].Url;

                            if (links[t].Pin == null
                                && tryProcessLinks)
                            {
                                if (links[t].Pin == null)
                                {
                                    var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(url);
                                    links[t].Url = newUrl;
                                    links[t].Pin = _pinHelper.TryToGenerateMapPin(newUrl);
                                    mapsCallCount++;
                                }

                            }
                            mapsLinkCount++;
                        }
                    }



                }

                if (topic.HasMapPin() && topic.HasMapLink())
                {
                    unProcessedUrlsCount++;
                    unProcessedUrls += topic.UrlsV2[0] + "/r/n";
                }

                if (topic.ResponseHasLink) responseLinkCount++;
                if (topic.ResponseHasMapLink) responseMapsLinkCount++;

                Console.WriteLine($"{topic.GetHashTags()}");
                Console.WriteLine($"{topic.Title} : {responseText}");
                Console.WriteLine("--------------------------------------");

                if (mapsCallCount > 100)
                {
                    Console.WriteLine("--------------------------------------");
                    //break;
                }

                pinCount += topic.UrlsV2.Where(o => o.Pin != null).Count();

                foreach (var response in topic.ResponsesV2)
                {
                    if (response.Links != null)
                    {
                        pinCount += response.Links.Where(o => o.Pin != null).Count();
                    }
                }


            }



            Console.WriteLine($"Pin Count : {pinCount}");
            Console.WriteLine($"Unprocessed Links : {unProcessedUrlsCount}");
            Console.WriteLine($"Has Links : {linkCount}");
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
            Console.WriteLine($"Has Response Links : {responseLinkCount}");
            Console.WriteLine($"Has Response Maps Links : {responseMapsLinkCount}");

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nStarting AI processing");
            //_aIProcessingService.AIProcessing(Topics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");

            _topicsHelper.SaveTopics(DBFileName, Topics);
        }

        void ReadFileLineByLine(string filePath)
        {
            // Open the file and read each line
            using (StreamReader sr = new StreamReader(filePath))
            {
                string? line;
                int i = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        var messages = line.Split(new string[] { "}/r/n" }, StringSplitOptions.None);
                        // Process the line
                        i++;
                        Console.WriteLine(i);
                        foreach (var message in messages)
                        {
                            try
                            {
                                GroupRoot? m;
                                if (messages.Length > 1 && message != messages[messages.Length - 1])
                                {
                                    m = JsonConvert.DeserializeObject<GroupRoot>(message + "}");
                                }
                                else
                                {
                                    m = JsonConvert.DeserializeObject<GroupRoot>(message);
                                }
                                ProcessModel(m);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(message);

                            }
                        }
                    }
                }
            }
        }

        private void ProcessModel(GroupRoot? groupRoot)
        {
            if (groupRoot == null) return;
            string? messageText;
            if (groupRoot == null || groupRoot.data.node == null) return;

            var a = groupRoot.data.node.comet_sections;
            var nodeId = groupRoot.data.node.id;
            if (a != null)
            {
                var story = a.content.story;
                messageText = story?.message?.text;

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    // TODO: Log?
                    return;
                }
                Topic? currentTopic = _topicHelper.GetOrCreateTopic(Topics, nodeId, messageText);

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
                currentTopic.FacebookUrl = story.wwwURL;
            }

        }

        private void CheckForUpdatedUrls(Topic topic, List<TopicLink> newUrls)
        {
            if (topic.UrlsV2 == null) return;
            if (topic.UrlsV2.Count != newUrls.Count)
            {
                Console.WriteLine("Mismatch in url detection");
            }
            else
            {
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    if (topic.UrlsV2[t].Pin == null)
                    {
                        if (topic.UrlsV2[t].Url != newUrls[t].Url
                            && !topic.UrlsV2[t].Url.Contains("/@")
                            && !topic.UrlsV2[t].Url.Contains("https://www.google.com/maps/d/viewer"))
                        {
                            topic.UrlsV2[t].Url = newUrls[t].Url;
                        }
                    }
                }
            }

        }

    }
}
