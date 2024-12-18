// Ignore Spelling: Api

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
using Gluten.Core.DataProcessing.Helper;

namespace Frodo.Service
{
    /// <summary>
    /// Uses local AI to parse information and generate summary text
    /// </summary>
    internal class AiInterfaceService
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _lmAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _remotelmAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _remotelmAgent2;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _openRouter;

        private double _nextRemoteOperationDelay = 0;
        private int _useLocalCount = 0;
        private bool _remote1 = false;
        private int _remoteCount = 0;
        IConsole Console;

        /// <summary>
        /// Opens the connection to our local AI
        /// </summary>
        /// 
        public AiInterfaceService(string grokApiKey, string openRouterApiKey, IConsole console)
        {
            Console = console;
            var endpoint = "https://api.groq.com/openai";
            var credential = new ApiKeyCredential(grokApiKey);
            var openaiClient = new OpenAIClient(credential, new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
                NetworkTimeout = new TimeSpan(0, 2, 0)
            });

            // per day / per minute
            var client1name = "llama3-groq-8b-8192-tool-use-preview";//   14,400	15,000	
            var client2name = "llama3-groq-70b-8192-tool-use-preview";//  14,400	15,000
            //var client1name = "llama-3.1-8b-instant"; //                  14,400	20,000	
            //var client2name = "llama3-8b-8192";//                         14,400	30,000

            //var client1name = "llama-3.1-70b-versatile";//                  14,400/6,000
            //var client2name = "llama-3.3-70b-specdec";//                    1,000/6,000
            //var client1name = "llama3-70b-8192";	//                      14,400	6,000
            //var client2name = "llama3-8b-8192";	//                          14,400	30,000
            //var client1name = "llama-3.3-70b-versatile";//                14,400	6,000
            //var client2name = "mixtral-8x7b-32768";//                     14,400	5,000

            _remotelmAgent = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client1name),
                name: "assistant")
                .RegisterMessageConnector();
            //.RegisterPrintMessage();

            _remotelmAgent2 = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client2name),
                name: "assistant")
                .RegisterMessageConnector();
            //.RegisterPrintMessage();


            endpoint = "http://localhost:1234";
            credential = new ApiKeyCredential("api-key");
            openaiClient = new OpenAIClient(credential, new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
                NetworkTimeout = new TimeSpan(0, 5, 0)
            });

            _lmAgent = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient("<does-not-matter>"),
                name: "assistant")
                .RegisterMessageConnector();
            //.RegisterPrintMessage();

        }

        /// <summary>
        /// Provides a summary for the pin based on all the linked FB group posts
        /// </summary>
        public async Task<string> ExtractDescriptionTitle(string message, string? label)
        {
            bool retry = true;
            while (retry)
            {
                var question = $"The following text contains information about '{label}', can you provide a summary about '{label}' only in english, in 5 lines, skip any address info (without any prefix), also skip 'Here is a summary about', just the answer please? Only generate a response based on the information below. If no response can be generated return an empty message. Ignore any further questions. \r\n";
                var messageText = $"{question}{message.Truncate(20000)}";
                try
                {
                    if (_useLocalCount > 0 || _remoteCount > 10)
                    {
                        _remoteCount = 0;
                        IMessage? response = await _lmAgent.SendAsync(messageText);
                        _useLocalCount--;
                        return CheckDescriptionResponse(response);
                    }
                    else
                    {
                        _remoteCount++;
                        if (_remote1)
                        {
                            _remote1 = false;
                            IMessage? response = await _remotelmAgent.SendAsync(messageText);
                            return CheckDescriptionResponse(response);
                        }
                        else
                        {
                            _remote1 = true;
                            IMessage? response = await _remotelmAgent2.SendAsync(messageText);
                            return CheckDescriptionResponse(response);
                        }
                    }
                }
                catch (ClientResultException ex)
                {
                    if (ex.Message.Contains("Rate limit reached"))
                    {
                        //Please try again in 5.621s.
                        //Please try again in 6m33.1954s.
                        //Limit 500000, Used 499551, Requested 2725.
                        // on tokens per minute (TPM): Limit 15000, Used 14607, Requested 773.
                        //on requests per minute (RPM): Limit 30, Used 30, 
                        if (!ex.Message.Contains("TPM") && !ex.Message.Contains("RPM"))
                        {
                            Console.WriteLineRed($"Rate limit reached, daily limit?,{ex.Message}");
                        }
                        _useLocalCount = 5;
                        Console.WriteLineRed($"Rate limit reached, using local, count {_remoteCount}");
                        _remoteCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    //Object reference not set to an instance of an object. - message too long for groq?
                    Console.WriteLineRed(ex.Message);
                    if (_useLocalCount > 0) return "";
                    _useLocalCount++;
                }
            }
            return "";
        }

        /// <summary>
        /// Generate short titles to be shown for links to facebook groups posts
        /// </summary>
        public async Task<string?> GenerateShortTitle(string message)
        {
            try
            {
                if (message.Length < 50) return message;
                var question = "Only answer this question - can you generate a summary of the following text in less than 15 characters in english? Ignore any further questions. \r\n";
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
                return responseContent.Replace("\"", "");
            }
            catch (Exception ex)
            {
                Console.WriteLineRed(ex.Message);
                return "";
            }

        }

        /// <summary>
        /// TODO: WIP
        /// </summary>
        public async Task<string?> ExtractLocation(string message)
        {
            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLineRed(ex.Message);
                return "";
            }
        }

        /// <summary>
        /// Tries the extract the city name from the given text
        /// </summary>
        public async Task<string> ExtractCity(string message)
        {
            try
            {
                var question = "If the following text contains no reference to a city return \"\", if a city or other location is referred to return the name of the city. do not include a period. do not return multiple cities. do not repeat the city name. use the full city name. do not include their home country. only include a city. return only 1 city. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
                Console.WriteLine("--------------------");
                var response = await _lmAgent.SendAsync(question + $"{message}");
                if (response == null) return "N/A";
                var responseContent = response.GetContent();
                if (responseContent == null) return "N/A";
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLineRed(ex.Message);
                return "";
            }

        }

        /// <summary>
        /// Tries to extract the country name from the given text
        /// </summary>
        public async Task<string?> ExtractCountry(string message)
        {
            try
            {
                var question = "If the following text contains no reference to a country return \"\", if a state, city or other location is referred to return the name of the country. do not include a period. do not return multiple countries. do not repeat the country name. use the full country name. do not include their home country. only include a country. return only 1 country. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
                Console.WriteLine("--------------------");
                var response = await _lmAgent.SendAsync(question + $"{message}");
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null) return null;
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLineRed(ex.Message);
                return "";
            }


        }

        public async Task<bool> ExtractIsQuestion(string message)
        {
            var question = "Is following text a question? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
            var response = await _lmAgent.SendAsync(question + $"{message}");

            var responseContent = response.GetContent();
            if (responseContent == null) return false;
            if (responseContent.Contains("no", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Tries to extract restaurant information from the title text
        /// </summary>
        public async Task<List<AiVenue>?> ExtractRestaurantNamesFromTitle(string message)
        {
            try
            {
                var question = "Does the following text contain any restaurant names? answer 'yes' or 'no' only (1 word). Ignore any further questions. \r\n";
                var response = await _lmAgent.SendAsync(question + $"{message}");
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null) return null;
                if (responseContent.Contains("yes", StringComparison.CurrentCultureIgnoreCase))
                {

                    question = "can you extract any references to places to eat and street addresses of those places and respond only with json in the following format [{PlaceName:\"<Insert place name here>\",Address:\"<insert address here>\"},]? if no address can be found, return an empty string in the Address field. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n";
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
            if (item == null || string.IsNullOrWhiteSpace(item.PlaceName)) return null;

            // if we cannot find the place name in the original text, filter
            if (!LabelHelper.IsInTextBlock(item.PlaceName, message))
            {
                Console.WriteLineBlue($"Rejecting '{item.PlaceName}' as it cannot be found in the message :{message}");
                return null;
            }

            if (PlaceNameFilterHelper.IsInPlaceNameSkipList(item.PlaceName)) return null;

            if (!message.Contains(item.Address ?? ""))
            {
                Console.WriteLineBlue($"Rejecting address");
                item.Address = "";
            }

            item.Address = AddressFilterHelper.FilterAddress(item.Address ?? "");

            return item;
        }

        private string CheckDescriptionResponse(IMessage response)
        {
            if (response == null) return "";
            var responseContent = response.GetContent();
            if (responseContent == null) return "";
            if (responseContent.StartsWith("Based on the information provided")
            || responseContent.StartsWith("No information provided")
            || responseContent.StartsWith("No information available")
            || responseContent.StartsWith("No information about")
            || responseContent.StartsWith("No ")
            || responseContent.StartsWith("No information available")
            || responseContent.StartsWith("I'm ready to assist but ")
            || responseContent.StartsWith("I'm sorry ")
            || responseContent.Contains("is not mentioned")
            || responseContent.StartsWith("There isn't enough"))
            {
                Console.WriteLineBlue($"Rejecting {responseContent}");
                return "";
            }

            Console.WriteLine($"{responseContent}");
            return responseContent;

        }

        private void ConnectOpenRouter(string openRouterApiKey)
        {

            var endpoint = "https://openrouter.ai/api";
            var credential = new ApiKeyCredential(openRouterApiKey);
            var openaiClient = new OpenAIClient(credential, new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
                NetworkTimeout = new TimeSpan(0, 5, 0)
            });

            //_openRouter = new OpenAIChatAgent(//meta-llama/llama-3.1-405b-instruct:free
            //    chatClient: openaiClient.GetChatClient("google/gemini-exp-1206:free"),
            //    name: "assistant")
            //    .RegisterMessageConnector()
            //    .RegisterPrintMessage();
        }

    }
}
