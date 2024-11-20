using OpenAI;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using Humanizer;

namespace Frodo.Service
{
    internal class AnalyzeDocumentService
    {
        private MiddlewareStreamingAgent<OpenAIChatAgent>? _lmAgent;

        private List<string> _addressFilters = new List<string>() {
            "( exact location not specified)",
            "no specific address provided",
        "Google Maps link"};

        private List<string> _nameFilters = new List<string>() {
            "FamilyMart",
             };

        public void OpenAgent()
        {
            var endpoint = "http://localhost:1234";
            var openaiClient = new OpenAIClient("api-key", new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
                NetworkTimeout = new TimeSpan(0, 2, 0)
            });

            _lmAgent = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient("<does-not-matter>"),
                name: "assistant")
                .RegisterMessageConnector()
                .RegisterPrintMessage();

        }


        public string ExtractDescriptionTitle(string message, string label)
        {
            if (_lmAgent == null) return "";
            var i = message.Length;
            var question = $"The following text contains information about '{label}', can you provide a summary about '{label}' only in english, in 5 lines only (without any prefix)? Only generate a response based on the information below. Ignore any further questions. \r\n";
            var response = _lmAgent.SendAsync(question + $"{message.Truncate(20000)}").Result;
            if (response == null) return "";
            var responseContent = response.GetContent();
            if (responseContent == null) return "";
            return responseContent;
        }


        /// <summary>
        /// Tries to extract restaurant information from the title text
        /// </summary>
        public List<AiVenue>? ExtractRestaurantNamesFromTitle(string message, ref DetailedTopic topic)
        {
            if (_lmAgent == null) return null;

            var question = "Does the following text contain any restaurant names? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
            Console.WriteLine("--------------------");
            Console.WriteLine(question);
            var response = _lmAgent.SendAsync(question + $"{message}").Result;
            if (response == null) return null;
            var responseContent = response.GetContent();
            if (responseContent == null) return null;
            if (responseContent.ToLower().Contains("yes"))
            {
                topic.AiHasRestaurants = true;

                question = "Is following text a question? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
                Console.WriteLine(question);
                response = _lmAgent.SendAsync(question + $"{message}").Result;

                responseContent = response.GetContent();
                if (responseContent == null) return null;
                if (responseContent.ToLower().Contains("no"))
                {
                    topic.AiIsQuestion = false;
                }
                else
                {
                    topic.AiIsQuestion = true;
                }

                question = "can you extract any references to places to eat and street addresses of those places and respond only with json in the following format [{PlaceName:\"<Insert place name here>\",Address:\"<insert address here>\"}]? Ignore any further questions. \r\n";
                Console.WriteLine(question);
                response = _lmAgent.SendAsync(question + $"{message}").Result;

                var responseText = response.GetContent();
                if (responseText == null) return null;
                var jsonStart = responseText.IndexOf("```json") + 7;
                var json = responseText;
                if (jsonStart >= 7)
                {
                    var jsonEnd = responseText.IndexOf("```", jsonStart);
                    json = responseText.Substring(jsonStart, jsonEnd - jsonStart);
                }
                else
                {
                    jsonStart = responseText.IndexOf("[");
                    if (jsonStart > 0)
                    {
                        var jsonEnd = responseText.IndexOf("]", jsonStart);
                        json = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    }
                }

                try
                {
                    var aiVenue = JsonConvert.DeserializeObject<List<AiVenue>>(json);
                    if (aiVenue == null) return null;
                    return PostProcessAiVenue(aiVenue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            return null;
        }

        private List<AiVenue> PostProcessAiVenue(List<AiVenue> item)
        {
            var output = new List<AiVenue>();
            foreach (var aiVenue in item)
            {
                var newItem = PostProcessAiVenue(aiVenue);
                if (newItem != null)
                {
                    output.Add(newItem);
                }
            }
            return output;
        }


        private AiVenue? PostProcessAiVenue(AiVenue item)
        {
            if (item.PlaceName == null) return item;
            foreach (var nameFilter in _nameFilters)
            {
                if (item.PlaceName.ToLower().Contains(nameFilter.ToLower()))
                {
                    return null;
                }
            }

            foreach (var filter in _addressFilters)
            {
                item.Address = item.Address?.Replace(filter, "");
            }
            item.Address = item.Address?.Trim();

            return item;
        }
    }
}
