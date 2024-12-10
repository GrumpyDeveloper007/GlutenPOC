using OpenAI;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using Humanizer;
using System.ClientModel;
using Frodo.Helper;
using Gluten.Core.Interface;

namespace Frodo.Service
{
    /// <summary>
    /// Uses local AI to parse information and generate summary text
    /// </summary>
    internal class LocalAiInterfaceService(IConsole Console)
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
            "Lovely restaurant"
        ];

        private readonly List<string> _nameFilters = [];

        /// <summary>
        /// Opens the connection to our local AI
        /// </summary>
        private void OpenAgent()
        {
            var endpoint = "http://localhost:1234";
            var credential = new ApiKeyCredential("api-key");
            var openaiClient = new OpenAIClient(credential, new OpenAIClientOptions
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
        public async Task<string> ExtractDescriptionTitle(string message, string? label)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return "";
            var question = $"The following text contains information about '{label}', can you provide a summary about '{label}' only in english, in 5 lines or less, skip any address info (without any prefix), also skip 'Here is a summary about', just the answer please? Only generate a response based on the information below. If no response can be generated return an empty message. Ignore any further questions. \r\n";
            var response = await _lmAgent.SendAsync(question + $"{message.Truncate(20000)}");
            if (response == null) return "";
            var responseContent = response.GetContent();
            if (responseContent == null) return "";
            if (responseContent.StartsWith("Based on the information provided")) return "";
            return responseContent;
        }

        /// <summary>
        /// Generate short titles to be shown for links to facebook groups posts
        /// </summary>
        public async Task<string?> GenerateShortTitle(string message)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return null;

            if (message.Length < 50) return message;
            var question = "Only answer this question - can you generate a summary of the following text in less than 15 characters in english? Ignore any further questions. \r\n";
            Console.WriteLine("--------------------");
            var response = await _lmAgent.SendAsync(question + $"{message}");
            if (response == null) return null;
            var responseContent = response.GetContent();
            responseContent = (responseContent ?? "").Replace("Yes, I can summarize that text in under 15 English characters:", "");
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
        /// TODO: WIP
        /// </summary>
        public async Task<string?> ExtractLocation(string message)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return null;

            var question = "If the following text contains no reference to a country, state, city name return \"\", do not include a period. return one of the following : \"\", country, state, city names only. do not guess, if the text does not specify return \"\". if this is not a known return \"\". do not repeat the location name. do not include their home country/location. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
            Console.WriteLine("--------------------");
            Console.WriteLine(question);
            var response = await _lmAgent.SendAsync(question + $"{message}");
            if (response == null) return null;
            var responseContent = response.GetContent();
            if (responseContent == null) return null;
            var location = responseContent;

            response = await _lmAgent.SendAsync($"is the following a country, state, city? return only yes/no \r\n{location}");
            if (response == null) return null;
            responseContent = response.GetContent();
            if (responseContent == null) return null;
            if (responseContent.Contains("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                return location;
            }

            return "";


        }

        /// <summary>
        /// Tries the extract the city name from the given text
        /// </summary>
        public async Task<string> ExtractCity(string message)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return "N/A";

            var question = "If the following text contains no reference to a city return \"\", if a city or other location is referred to return the name of the city. do not include a period. do not return multiple cities. do not repeat the city name. use the full city name. do not include their home country. only include a city. return only 1 city. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
            Console.WriteLine("--------------------");
            var response = await _lmAgent.SendAsync(question + $"{message}");
            if (response == null) return "N/A";
            var responseContent = response.GetContent();
            if (responseContent == null) return "N/A";
            return responseContent;

        }

        /// <summary>
        /// Tries to extract the country name from the given text
        /// </summary>
        public async Task<string?> ExtractCountry(string message)
        {
            if (_lmAgent == null) OpenAgent();
            if (_lmAgent == null) return null;

            var question = "If the following text contains no reference to a country return \"\", if a state, city or other location is referred to return the name of the country. do not include a period. do not return multiple countries. do not repeat the country name. use the full country name. do not include their home country. only include a country. return only 1 country. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
            Console.WriteLine("--------------------");
            var response = await _lmAgent.SendAsync(question + $"{message}");
            if (response == null) return null;
            var responseContent = response.GetContent();
            if (responseContent == null) return null;
            return responseContent;

        }

        /// <summary>
        /// Tries to extract restaurant information from the title text
        /// </summary>
        public async Task<List<AiVenue>?> ExtractRestaurantNamesFromTitle(string message, DetailedTopic topic)
        {
            try
            {
                if (_lmAgent == null) OpenAgent();
                if (_lmAgent == null) return null;

                var question = "Does the following text contain any restaurant names? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
                Console.WriteLine("--------------------");
                Console.WriteLine(question);
                var response = await _lmAgent.SendAsync(question + $"{message}");
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null) return null;
                if (responseContent.Contains("yes", StringComparison.CurrentCultureIgnoreCase))
                {
                    topic.AiHasRestaurants = true;

                    question = "Is following text a question? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
                    Console.WriteLine(question);
                    response = await _lmAgent.SendAsync(question + $"{message}");

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

                    question = "can you extract any references to places to eat and street addresses of those places and respond only with json in the following format [{PlaceName:\"<Insert place name here>\",Address:\"<insert address here>\"},]? if no address can be found return \"\" in the Address field. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n";
                    Console.WriteLine(question);
                    response = await _lmAgent.SendAsync(question + $"{message}");

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
                        return PostProcessAiVenue(aiVenue, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                else
                {
                    topic.AiIsQuestion = true;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private List<AiVenue> PostProcessAiVenue(List<AiVenue> item, string message)
        {
            var output = new List<AiVenue>();
            foreach (var aiVenue in item)
            {
                var newItem = PostProcessAiVenue(aiVenue, message);
                if (newItem != null)
                {
                    output.Add(newItem);
                }
            }
            return output;
        }

        private AiVenue? PostProcessAiVenue(AiVenue item, string message)
        {
            if (item == null || item.PlaceName == null) return null;
            foreach (var nameFilter in _nameFilters)
            {
                if (item.PlaceName.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase))
                {
                    return null;
                }
            }

            if (!message.Contains(item.Address ?? ""))
            {
                Console.WriteLineBlue($"Rejecting address");
                item.Address = "";
            }

            foreach (var filter in _addressFilters)
            {
                item.Address = item.Address?.Replace(filter, "");
            }
            item.Address = item.Address?.Trim();

            // if we cannot find the place name in the original text, filter
            if (!LabelHelper.IsInTextBlock(item.PlaceName, message))
            {
                Console.WriteLineBlue($"Rejecting '{item.PlaceName}' as it cannot be found in the message :{message}");

                return null;
            }

            return item;
        }


    }
}
