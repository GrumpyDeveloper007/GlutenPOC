using Frodo.FacebookModel;
using Frodo.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Frodo.Service
{
    /// <summary>
    /// Scan response information collected from FB, do some basic processing
    /// </summary>
    internal class DataSyncService
    {
        private TopicHelper _topicHelper = new TopicHelper();

        public List<Topic> Topics = new List<Topic>();
        private string DBFileName = "D:\\Coding\\Gluten\\Topics.json";

        public void ProcessFile()
        {
            string json;
            if (File.Exists(DBFileName))
            {
                json = File.ReadAllText(DBFileName);
                var tempTopics = JsonConvert.DeserializeObject<List<Topic>>(json);
                if (tempTopics != null) { Topics = tempTopics; }
            }

            ReadFileLineByLine("D:\\Coding\\Gluten\\Smeagol\\bin\\Debug\\net8.0\\Responses.txt");

            int i = 0;
            int linkCount = 0;
            int mapsLinkCount = 0;
            int responseLinkCount = 0;
            int responseMapsLinkCount = 0;
            int mapsCallCount = 0;
            string unProcessedUrls = "";
            int unProcessedUrlsCount = 0;
            bool tryProcessLinks = true;
            foreach (var topic in Topics)
            {
                i++;
                string responseText = "";
                foreach (var response in topic.ResponsesV2)
                {
                    responseText += response + " , ";
                }
                topic.HashTags = StringHelper.ExtractHashtags(topic.Title);
                topic.Urls = StringHelper.ExtractUrls(topic.Title);
                if (topic.HasLink) linkCount++;
                if (topic.HasMapLink && topic.TopicPin == null)
                {
                    for (int t = 0; t < topic.Urls.Count; t++)
                    {
                        var url = topic.Urls[t];

                        // https://www.google.com/maps/place/onwa/@34.6785478,135.8161308,17z/data=!3m1!4b1!4m6!3m5!1s0x60013a30562e78d3:0xd712400d34ea1a7b!8m2!3d34.6785478!4d135.8161308!16s%2Fg%2F11f3whn720!5m1!1e4?entry=ttu&g_ep=EgoyMDI0MTAwOS4wIKXMDSoASAFQAw%3D%3D
                        //"https://maps.app.goo.gl/jzpnWbttyQVicafZ9?g_st=il"
                        //"https://maps.google.com/?q=Giappone,%20%E3%80%92605-0826%20Kyoto,%20Higashiyama%20Ward,%20Masuyacho,%20349-21%20%E3%82%B8%E3%83%93%E3%82%A8%E3%83%BB%E6%B4%BB%E5%B7%9D%E9%AD%9A%E6%96%99%E7%90%86%E3%83%BB%E5%8D%81%E5%89%B2%E6%89%8B%E6%89%93%E3%81%A1%E8%95%8E%E9%BA%A6%E5%87%A6%20%E3%80%8C%E6%94%BF%E5%8F%B3%E8%A1%9B%E9%96%80%E3%80%8D(MASAEMON)&ftid=0x600109a72b6b2469:0xa0a247525adfb00f&entry=gps&lucs=,94224825,94227247,94227248,47071704,47069508,94218641,94203019,47084304,94208458,94208447&g_st=com.google.maps.preview.copy):"
                        //"https://www.google.com/maps/reviews/data=!4m8!14m7!1m6!2m5!1sChZDSUhNMG9nS0VJQ0FnSUNqbHRqY1JREAE!2m1!1s0x0:0x48f8753e21338831!3m1!1s2"
                        // Link to street view
                        //https://maps.google.com/maps/api/staticmap?center=34.6845322%2C135.1840363&amp;zoom=-1&amp;size=900x900&amp;language=en&amp;sensor=false&amp;client=google-maps-frontend&amp;signature=yGPXtu3-Vjroz_DtJZLPyDkVVC8\
                        if (url.Contains("www.google.com/maps/")
                            || url.Contains("maps.app.goo.gl")
                            || url.Contains("maps.google.com"))
                        {
                            if (!url.Contains("/@") && tryProcessLinks)
                            {
                                mapsCallCount++;
                                if (mapsCallCount > 10) tryProcessLinks = false;
                                var mapsHelper = new MapsUrlProcessor();
                                if (!mapsHelper.ProcessMapsUrl(topic, t))
                                    tryProcessLinks = false;
                            }
                        }
                        url = topic.Urls[t];

                        TryToGenerateMapPin(topic, t);

                        mapsLinkCount++;
                    }
                    if (topic.TopicPin == null)
                    {
                        unProcessedUrlsCount++;
                        unProcessedUrls += topic.Urls[0] + "/r/n";
                    }

                }
                if (topic.ResponseHasLink) responseLinkCount++;
                if (topic.ResponseHasMapLink) responseMapsLinkCount++;

                if (i < 10)
                {
                    Console.WriteLine($"{topic.GetHashTags()}");
                    Console.WriteLine($"{topic.Title} : {responseText}");
                    Console.WriteLine("--------------------------------------");
                }

            }

            Console.WriteLine($"Unprocessed Links : {unProcessedUrlsCount}");
            Console.WriteLine($"Has Links : {linkCount}");
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
            Console.WriteLine($"Has Response Links : {responseLinkCount}");
            Console.WriteLine($"Has Response Maps Links : {responseMapsLinkCount}");

            json = JsonConvert.SerializeObject(Topics);
            File.WriteAllText(DBFileName, json);

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
            }

        }

        private void TryToGenerateMapPin(Topic topic, int t)
        {
            if (topic.Urls == null) return;
            var url = topic.Urls[t];

            if (url.Contains("/@"))
            {
                var left = url.IndexOf("/@") + 2;
                var latEnd = url.IndexOf(",", left);
                var longEnd = url.IndexOf(",", latEnd + 1);

                var lat = url.Substring(left, latEnd - left);
                var lon = url.Substring(latEnd + 1, longEnd - latEnd - 1);

                var placeStart = url.IndexOf("/place/") + "/place/".Length;
                var placeEnd = url.IndexOf("/", placeStart);

                //"https://www.google.com/maps/preview/place/Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyacho,+3%E2%88%9223+onwa/@34.6785478,135.8161308,3281a,13.1y/data\\\\u003d!4m2!3m1!1s0x60013a30562e78d3:0xd712400d34ea1a7b\\"
                //                                          "Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyac"
                var placeName = url.Substring(placeStart, placeEnd - placeStart);
                placeName = HttpUtility.UrlDecode(placeName);

                topic.TopicPin = new TopicPin();
                topic.TopicPin.Label = placeName;
                topic.TopicPin.Address = placeName;
                topic.TopicPin.GeoLatatude = lat;
                topic.TopicPin.GeoLongitude = lon;
            }
        }

    }
}
