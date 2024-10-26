using OpenAI;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Frodo.Service
{
    internal class AnalyzeDocumentService
    {
        OpenAIChatAgent _openAIChatAgent;

        public List<AiVenue> test(string message, ref Topic topic)
        {
            var endpoint = "http://localhost:1234";
            var openaiClient = new OpenAIClient("api-key", new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
            });

            var lmAgent = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient("<does-not-matter>"),
                name: "assistant")
                .RegisterMessageConnector()
                .RegisterPrintMessage();
            //_openAIChatAgent = lmAgent;

            var question = "Does the following text contain any restaurant names, answer 'yes' or 'no' only? \r\n";
            Console.WriteLine("--------------------");
            Console.WriteLine(question);
            var response = lmAgent.SendAsync(question + $"{message}").Result;
            if (response.GetContent().ToLower().Contains("yes"))
            {
                topic.AiHasRestaurants = true;

                question = "Is following text a question, answer 'yes' or 'no' only? \r\n";
                Console.WriteLine(question);
                response = lmAgent.SendAsync(question + $"{message}").Result;

                if (response.GetContent().ToLower().Contains("no"))
                {
                    topic.AiIsQuestion = false;
                }
                else
                {
                    topic.AiIsQuestion = true;
                }

                question = "can you extract any references to places to eat and street addresses of those places and respond only with json in the following format [{PlaceName:\"<Insert place name here>\",Address:\"<insert address here>\"}]? Ignore any further questions. \r\n";
                Console.WriteLine(question);
                response = lmAgent.SendAsync(question + $"{message}").Result;

                var responseText = response.GetContent();
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
                    return PostProcessAiVenue(aiVenue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            return null;

            //response = await lmAgent.SendAsync();
        }


        private List<string> _addressFilters = new List<string>() {
            "( exact location not specified)",
            "no specific address provided",
        "Google Maps link"};

        private List<string> _nameFilters = new List<string>() {
            "FamilyMart",
             };

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


        private AiVenue PostProcessAiVenue(AiVenue item)
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
