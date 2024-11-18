using AutoMapper;
using Azure.AI.TextAnalytics;
using Frodo.FacebookModel;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using System;

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
        private AnalyzeDocumentService _analyzeDocumentService = new AnalyzeDocumentService();
        private PinHelper _pinHelper = new PinHelper();
        private AIProcessingService _aIProcessingService;

        public List<DetailedTopic> Topics = new List<DetailedTopic>();
        private string DBFileName = "D:\\Coding\\Gluten\\Topics.json";
        private string ExportDBFileName = "D:\\Coding\\Gluten\\TopicsExport.json";

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

            _analyzeDocumentService.OpenAgent();

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
                            topic.UrlsV2[t].Pin = _pinHelper.TryToGenerateMapPin(newUrl, false);
                            mapsCallCount++;
                        }

                    }
                    mapsLinkCount++;
                }

                // Look for links in the responses
                for (int z = 0; z < topic.ResponsesV2.Count; z++)
                {
                    var message = topic.ResponsesV2[z].Message;
                    if (message == null) continue;

                    var newLinks = StringHelper.ExtractUrls(message);
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
                                    links[t].Pin = _pinHelper.TryToGenerateMapPin(newUrl, false);
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
            for (int i = 0; i < Topics.Count; i++)
            {
                if (i < 0)
                {
                    Console.WriteLine(i);
                    DetailedTopic? topic = Topics[i];
                    var venue = _analyzeDocumentService.ExtractRestaurantNamesFromTitle(topic.Title, ref topic);
                    topic.AiVenues = venue;
                    _topicsHelper.SaveTopics(DBFileName, Topics);
                }

            }


            _topicsHelper.SaveTopics(DBFileName, Topics);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DetailedTopic, Topic>();
                cfg.CreateMap<DetailedTopic, PinLinkInfo>();
                cfg.CreateMap<Topic, PinLinkInfo>();
            });

            var mapper = config.CreateMapper();
            List<Topic> tempTopics = new List<Topic>();
            foreach (var t in Topics)
            {
                var newT = mapper.Map<Topic>(t);
                tempTopics.Add(newT);
            }
            var pins = ExtractPinExport(tempTopics, mapper);

            var ii = 0;
            foreach (var pin in pins)
            {
                var message = "";
                foreach (var item in pin.Topics)
                {
                    message += " " + item.Title;
                }
                pin.Description = _analyzeDocumentService.ExtractDescriptionTitle(message, pin.Label);
                _topicsHelper.SaveTopics<List<PinTopic>>(ExportDBFileName, pins);
                Console.WriteLine($" {ii}");
                ii++;
            }

            _topicsHelper.SaveTopics<List<PinTopic>>(ExportDBFileName, pins);

            //_topicsHelper.ExportTopics(ExportDBFileName, tempTopics);

            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"\r\nComplete, exit...");
        }

        private List<PinTopic> ExtractPinExport(List<Topic> topics, IMapper mapper)
        {
            var pins = new List<PinTopic>();
            foreach (var topic in topics)
            {
                var newT = mapper.Map<PinLinkInfo>(topic);
                if (topic.AiVenues != null)
                {
                    foreach (var aiVenue in topic.AiVenues)
                    {
                        var existingPin = IsPinInList(aiVenue.Pin, pins);
                        AddIfNotExists(pins, existingPin, newT, aiVenue.Pin);
                    }
                }

                if (topic.UrlsV2 != null)
                {
                    foreach (var url in topic.UrlsV2)
                    {
                        var existingPin = IsPinInList(url.Pin, pins);
                        AddIfNotExists(pins, existingPin, newT, url.Pin);
                    }
                }
            }
            return pins;
        }

        private void AddIfNotExists(List<PinTopic> pins, PinTopic? matchingPinTopic, PinLinkInfo topicToAdd, TopicPin? pinToAdd)
        {
            if (pinToAdd == null) return;
            if (pinToAdd == null) return;
            if (string.IsNullOrEmpty(pinToAdd.GeoLatatude)) return;
            if (string.IsNullOrEmpty(pinToAdd.GeoLongitude)) return;

            // if pin not found, add it to the list
            if (matchingPinTopic == null)
            {
                pins.Add(new PinTopic
                {
                    GeoLatatude = double.Parse(pinToAdd.GeoLatatude),
                    GeoLongitude = double.Parse(pinToAdd.GeoLongitude),
                    Label = pinToAdd.Label,
                    Topics = new List<PinLinkInfo> { topicToAdd }
                });
                return;
            }
            // dont add duplicates
            if (matchingPinTopic.Topics == null) matchingPinTopic.Topics = new List<PinLinkInfo>();
            foreach (var existingTopic in matchingPinTopic.Topics)
            {
                if (existingTopic.NodeID == topicToAdd.NodeID)
                {
                    return;
                }
            }
            matchingPinTopic.Topics.Add(topicToAdd);
        }

        private PinTopic? IsPinInList(TopicPin? topicPin, List<PinTopic> pins)
        {
            foreach (var pin in pins)
            {
                if (topicPin != null &&
                    topicPin.GeoLatatude != null &&
                    topicPin.GeoLongitude != null &&
                    pin.GeoLatatude == double.Parse(topicPin.GeoLatatude) && pin.GeoLongitude == double.Parse(topicPin.GeoLongitude))
                {
                    return pin;
                }
            }
            return null;
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
                DetailedTopic? currentTopic = _topicHelper.GetOrCreateTopic(Topics, nodeId, messageText);

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
            }

        }

        private void CheckForUpdatedUrls(DetailedTopic topic, List<TopicLink> newUrls)
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
