using Frodo.FacebookModel;
using Frodo.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor = new SeleniumMapsUrlProcessor();

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

            _seleniumMapsUrlProcessor.Start();

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
                            var newUrl = CheckUrlForMapLinks(url);
                            topic.UrlsV2[t].Url = newUrl;
                            topic.UrlsV2[t].Pin = TryToGenerateMapPin(newUrl);
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
                                    var newUrl = CheckUrlForMapLinks(url);
                                    links[t].Url = newUrl;
                                    links[t].Pin = TryToGenerateMapPin(newUrl);
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

            json = JsonConvert.SerializeObject(Topics, Formatting.Indented,
           new JsonConverter[] { new StringEnumConverter() });
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

        private TopicPin? TryToGenerateMapPin(string url)
        {
            if (url == null) return null;
            if (url.Contains("/@"))
            {
                var left = url.IndexOf("/@") + 2;
                var latEnd = url.IndexOf(",", left);
                var longEnd = url.IndexOf(",", latEnd + 1);

                if (latEnd > 0)
                {

                    var lat = url.Substring(left, latEnd - left);
                    var lon = url.Substring(latEnd + 1, longEnd - latEnd - 1);

                    var placeStart = url.IndexOf("/place/") + "/place/".Length;
                    var placeEnd = url.IndexOf("/", placeStart);

                    //"https://www.google.com/maps/preview/place/Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyacho,+3%E2%88%9223+onwa/@34.6785478,135.8161308,3281a,13.1y/data\\\\u003d!4m2!3m1!1s0x60013a30562e78d3:0xd712400d34ea1a7b\\"
                    //                                          "Japan,+%E3%80%92630-8123+Nara,+Sanjoomiyac"
                    var placeName = url.Substring(placeStart, placeEnd - placeStart);
                    placeName = HttpUtility.UrlDecode(placeName);

                    return new TopicPin()
                    {
                        Label = placeName,
                        Address = placeName,
                        GeoLatatude = lat,
                        GeoLongitude = lon
                    };
                }
            }
            return null;
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

        private string CheckUrlForMapLinks(string url)
        {
            // https://www.google.com/maps/place/onwa/@34.6785478,135.8161308,17z/data=!3m1!4b1!4m6!3m5!1s0x60013a30562e78d3:0xd712400d34ea1a7b!8m2!3d34.6785478!4d135.8161308!16s%2Fg%2F11f3whn720!5m1!1e4?entry=ttu&g_ep=EgoyMDI0MTAwOS4wIKXMDSoASAFQAw%3D%3D
            //"https://maps.app.goo.gl/jzpnWbttyQVicafZ9?g_st=il"
            //"https://maps.google.com/?q=Giappone,%20%E3%80%92605-0826%20Kyoto,%20Higashiyama%20Ward,%20Masuyacho,%20349-21%20%E3%82%B8%E3%83%93%E3%82%A8%E3%83%BB%E6%B4%BB%E5%B7%9D%E9%AD%9A%E6%96%99%E7%90%86%E3%83%BB%E5%8D%81%E5%89%B2%E6%89%8B%E6%89%93%E3%81%A1%E8%95%8E%E9%BA%A6%E5%87%A6%20%E3%80%8C%E6%94%BF%E5%8F%B3%E8%A1%9B%E9%96%80%E3%80%8D(MASAEMON)&ftid=0x600109a72b6b2469:0xa0a247525adfb00f&entry=gps&lucs=,94224825,94227247,94227248,47071704,47069508,94218641,94203019,47084304,94208458,94208447&g_st=com.google.maps.preview.copy):"
            //"https://www.google.com/maps/reviews/data=!4m8!14m7!1m6!2m5!1sChZDSUhNMG9nS0VJQ0FnSUNqbHRqY1JREAE!2m1!1s0x0:0x48f8753e21338831!3m1!1s2"
            //https://goo.gl/maps/QPEHvcC3svjdtf5G9
            // Link to street view
            //https://maps.google.com/maps/api/staticmap?center=34.6845322%2C135.1840363&amp;zoom=-1&amp;size=900x900&amp;language=en&amp;sensor=false&amp;client=google-maps-frontend&amp;signature=yGPXtu3-Vjroz_DtJZLPyDkVVC8\
            // Collection of pins 
            //https://www.google.com/maps/d/viewer?mid=16xtxMz-iijlDOEl-dlQKEa2-A19nxzND&ll=35.67714795882308,139.72588715&z=12
            //
            if (!url.Contains("https://www.google.com/maps/d/viewer")
                && (url.Contains("www.google.com/maps/")
                || url.Contains("maps.app.goo.gl")
                || url.Contains("maps.google.com")
                || url.Contains("https://goo.gl/maps/"))
                )
            {
                if (!url.Contains("/@"))
                {
                    var newUrl = _seleniumMapsUrlProcessor.GoAndWaitForUrlChange(url);
                    newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
                    newUrl = HttpUtility.UrlDecode(newUrl);
                    int timeout = 10;
                    while (!newUrl.Contains("/@") && timeout > 0)
                    {
                        timeout--;
                        System.Threading.Thread.Sleep(500);
                        newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
                        newUrl = HttpUtility.UrlDecode(newUrl);
                    }
                    if (timeout == 0 && !newUrl.Contains("/@"))
                    {
                        Console.WriteLine("Timeout waiting for url");
                        newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
                        newUrl = HttpUtility.UrlDecode(newUrl);
                    }

                    return newUrl;

                    //if (mapsCallCount > 10) tryProcessLinks = false;
                    //var mapsHelper = new MapsUrlProcessor();
                    //if (!mapsHelper.ProcessMapsUrl(topic, t))
                    //  tryProcessLinks = false;
                }
            }
            return url;

        }

    }
}
