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
    /// <summary>
    /// Uses local AI to parse information and generate summary text
    /// </summary>
    internal class LocalAiInterfaceService
    {
        private MiddlewareStreamingAgent<OpenAIChatAgent>? _lmAgent;

        // TODO: Clean up multiple filter lists
        private readonly List<string> _addressFilters = [
            "( exact location not specified)",
            "(No specific address mentioned)",
            "(no address provided)",
            "(no specific address given)",
            "(no street address provided in the text)",
            "<No specific address provided in the given text>",
            "<no address provided in the original text>",
            "<address not provided in original text>",
            "<Not provided in original text>",
            "<insert address here>",
            "<insert address>",
            "<unknown>",
            "<not provided>",
            "no specific address provided",
            "Not specified",
            "Google Maps link",
        ];

        private readonly List<string> _nameFilters = [];

        /// <summary>
        /// Opens the connection to our local AI
        /// </summary>
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

        /// <summary>
        /// Provides a summary for the pin based on all the linked FB group posts
        /// </summary>
        public string ExtractDescriptionTitle(string message, string? label)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return "";
            var question = $"The following text contains information about '{label}', can you provide a summary about '{label}' only in english, in 5 lines or less, skip any address info (without any prefix)? Only generate a response based on the information below. Ignore any further questions. \r\n";
            var response = _lmAgent.SendAsync(question + $"{message.Truncate(20000)}").Result;
            if (response == null) return "";
            var responseContent = response.GetContent();
            if (responseContent == null) return "";
            return responseContent;
        }

        /// <summary>
        /// Generate short titles to be shown for links to facebook groups posts
        /// </summary>
        public string? GenerateShortTitle(string message)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return null;

            if (message.Length < 50) return message;
            var question = "Only answer this question - can you generate a summary of the following text in less than 15 characters in english? Ignore any further questions. \r\n";
            Console.WriteLine("--------------------");
            var response = _lmAgent.SendAsync(question + $"{message}").Result;
            if (response == null) return null;
            var responseContent = response.GetContent();
            if (responseContent == null
                || responseContent.StartsWith("no ", StringComparison.InvariantCultureIgnoreCase)
                || responseContent.StartsWith("yes", StringComparison.InvariantCultureIgnoreCase)
                || responseContent.StartsWith("I'm sorry", StringComparison.InvariantCultureIgnoreCase)
                || responseContent.StartsWith("I apologize,", StringComparison.InvariantCultureIgnoreCase)
                || responseContent.Length > "No, I cannot generate a summary of that Facebook video link in under 15 characters. The text you provided is not actual".Length

                )
            {
                return null;
            }
            if (responseContent == null) return null;
            return responseContent;
        }

        /// <summary>
        /// Tries to extract restaurant information from the title text
        /// </summary>
        public List<AiVenue>? ExtractRestaurantNamesFromTitle(string message, ref DetailedTopic topic)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return null;

            var question = "Does the following text contain any restaurant names? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
            Console.WriteLine("--------------------");
            Console.WriteLine(question);
            var response = _lmAgent.SendAsync(question + $"{message}").Result;
            if (response == null) return null;
            var responseContent = response.GetContent();
            if (responseContent == null) return null;
            if (responseContent.Contains("yes", StringComparison.CurrentCultureIgnoreCase))
            {
                topic.AiHasRestaurants = true;

                question = "Is following text a question? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
                Console.WriteLine(question);
                response = _lmAgent.SendAsync(question + $"{message}").Result;

                responseContent = response.GetContent();
                if (responseContent == null) return null;
                if (responseContent.Contains("no", StringComparison.CurrentCultureIgnoreCase))
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
                    jsonStart = responseText.IndexOf('[');
                    if (jsonStart > 0)
                    {
                        var jsonEnd = responseText.IndexOf(']', jsonStart);
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
                if (item.PlaceName.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase))
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
