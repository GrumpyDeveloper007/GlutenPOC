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
using TimeSpanParserUtil;
using System;

namespace Frodo.Service
{
    /// <summary>
    /// Uses local AI to parse information and generate summary text
    /// </summary>
    internal class AiInterfaceService
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _lmAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent>[] _remotelmAgent = new MiddlewareStreamingAgent<OpenAIChatAgent>[15];
        private readonly DateTimeOffset[] _remoteNextAvailable = new DateTimeOffset[15];

        private int _useLocalCount = 0;
        private int _remoteIndex = 0;
        private int _remoteCount = 0;
        private readonly IConsole Console;

        /// <summary>
        /// Opens the connection to our local AI
        /// </summary>
        /// 
        public AiInterfaceService(string grokApiKey, IConsole console)
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
            var client3name = "llama-3.1-8b-instant"; //                  14,400	20,000	
            var client4name = "llama3-8b-8192";//                         14,400	30,000
            var client5name = "llama-3.3-70b-versatile";//                14,400	6,000
            var client6name = "llama-3.1-70b-versatile";//                  14,400/6,000
            var client7name = "llama3-70b-8192";	//                      14,400	6,000
            var client8name = "mixtral-8x7b-32768";//                     14,400	5,000
            //var client9name = "gemma-7b-it";                       //	30	14,400	15,000	500,000
            var client9name = "gemma2-9b-it";                     //	30	14,400	15,000	500,000
            //var client11name = "llama-3.2-11b-text-preview";       //	30	7,000	7,000	500,000
            var client10name = "llama-3.2-11b-vision-preview";     //	30	7,000	7,000	500,000
            var client11name = "llama-3.2-1b-preview";             //	30	7,000	7,000	500,000
            var client12name = "llama-3.2-3b-preview";             //	30	7,000	7,000	500,000
            //var client15name = "llama-3.2-90b-text-preview";       //	30	7,000	7,000	500,000
            var client13name = "llama-3.3-70b-specdec";            //	30	1,000	6,000	100,000
            var client14name = "llama-guard-3-8b";                 //	30	14,400	15,000	500,000
            var client15name = "llama3-8b-8192";                   //	30	14,400	30,000	500,000
            //llama-3.2-11b-text-preview
            //var client19name = "llava-v1.5-7b-4096-preview";         //	30	14,400	30,000	(No limit)

            //llama-3.2-90b-vision-preview	15	3,500	7,000	250,000

            //var client2name = "llama-3.3-70b-specdec";//                    1,000/6,000

            _remotelmAgent[0] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client1name),
                name: "assistant")
                .RegisterMessageConnector();
            //.RegisterPrintMessage();

            _remotelmAgent[1] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client2name),
                name: "assistant")
                .RegisterMessageConnector();
            //.RegisterPrintMessage();
            _remotelmAgent[2] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client3name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[3] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client4name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[4] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client5name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[5] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client6name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[6] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client7name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[7] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client8name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[8] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client9name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[9] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client10name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[10] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client11name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[11] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client12name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[12] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client13name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[13] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client14name),
                name: "assistant")
                .RegisterMessageConnector();
            _remotelmAgent[14] = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(client15name),
                name: "assistant")
                .RegisterMessageConnector();


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

            var question = $"The following text contains information about '{label}' (note, the exact text '{label}' may be present, it may be written slightly differently), can you provide a summary about '{label}' only in english, in 5 lines, skip any address info (without any prefix), also skip 'Here is a summary about', just the answer please? Only generate a response based on the information below. If no response can be generated return an empty message. Ignore any further questions. \r\n";
            var messageText = $"{question}{message.Truncate(20000)}";
            IMessage? response = await SendLBMessage(messageText);
            return CheckDescriptionResponse(response);
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
                var response = await SendLBMessage(question + $"{message}");
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

        public async Task<string?> CategoriseMessage(string message)
        {
            try
            {
                var question = "Only answer this question - Can you categorise the following text in to one of these options, describing one or more places, asking a question about a place, or unknown. Reply with only DESCRIBE,QUESTION,UNKNOWN. Ignore any further questions. \r\n";

                var response = await SendLBMessage(question + $"{message}");
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null)
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
                var question = "If the following text contains no reference to a city return \"\", if a city or other location is referred to return the name of the city. do not include a period. do not return multiple cities. do not repeat the city name. use the full city name. do not include their home country. only include one city, if multiple cities are referenced return \"\" (empty string). return only 1 city. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
                Console.WriteLine("--------------------");
                var response = await SendLBMessage(question + $"{message}");
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
                var response = await SendLBMessage(question + $"{message}");
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
            var response = await SendLBMessage(question + $"{message}");
            if (response == null) return false;
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
                var response = await SendLBMessage(question + $"{message}");
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null) return null;
                if (responseContent.Contains("yes", StringComparison.CurrentCultureIgnoreCase))
                {
                    var retry = true;
                    while (retry)
                    {
                        var responseText = "<tool_call>";

                        while (responseText.Contains("<tool_call>"))
                        {
                            question = "can you extract any references to places to eat and street addresses of those places and respond only with json in the following format [{PlaceName:\"<Insert place name here>\",Address:\"<insert address here>\",City:\"<insert city here>\"},]? if no address can be found, return an empty string in the Address field. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n";
                            response = await SendLBMessage(question + $"{message}");
                            if (response == null) return null;
                            responseText = response.GetContent();
                            if (responseText == null) return null;
                        }
                        if (responseText.StartsWith("I'm sorry")) return null;

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
                        catch (JsonReaderException ex)
                        {
                            Console.WriteLineRed($"Retrying json exception : {ex.Message}");
                        }
                        catch (JsonSerializationException ex)
                        {
                            Console.WriteLineRed($"Retrying json exception :{ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("Unexpected end when deserializing")
                                && !ex.Message.Contains("Additional text encountered after finished reading"))
                            {
                                Console.WriteLine(ex.Message);
                                retry = false;
                            }
                        }

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

            if (PlaceNameFilterHelper.StartsWithPlaceNameSkipList(item.PlaceName)) return null;

            if (!message.Contains(item.Address ?? ""))
            {
                //Console.WriteLineBlue($"Rejecting address");
                item.Address = "";
            }

            //No City Found
            //TODO: Check city against DB
            if (item.City != null && item.City.Contains("Found", StringComparison.InvariantCultureIgnoreCase))
            {
                item.City = "";
            }

            item.Address = AddressFilterHelper.FilterAddress(item.Address ?? "");

            return item;
        }

        private string CheckDescriptionResponse(IMessage? response)
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

        private async Task<IMessage?> SendLBMessage(string messageText)
        {
            bool retry = true;
            while (retry)
            {
                var currentIndex = _remoteIndex;
                try
                {
                    if (_useLocalCount > 0)
                    {
                        _remoteCount = 0;
                        IMessage? response = await _lmAgent.SendAsync(messageText);
                        _useLocalCount--;
                        return response;
                    }
                    else
                    {
                        bool allUnavailable = true;
                        for (int i = 0; i < _remotelmAgent.Count(); i++)
                        {
                            DateTimeOffset item = _remoteNextAvailable[i];
                            if (item <= DateTimeOffset.UtcNow)
                            {
                                allUnavailable = false;
                                break;
                            }
                        }
                        if (allUnavailable)
                        {
                            Console.WriteLineBlue($"All remotes are busy, using local");
                            _useLocalCount += 1;
                        }

                        if (_remoteNextAvailable[currentIndex] > DateTimeOffset.UtcNow)
                        {
                            _remoteIndex++;
                            if (_remoteIndex == _remotelmAgent.Count()) _remoteIndex = 0;
                            continue;
                        }

                        _remoteCount++;
                        IMessage? response;
                        response = await _remotelmAgent[_remoteIndex].SendAsync(messageText);
                        _remoteIndex++;
                        if (_remoteIndex == _remotelmAgent.Count()) _remoteIndex = 0;
                        return response;
                    }
                }
                catch (ClientResultException ex)
                {
                    _remoteIndex++;
                    if (_remoteIndex == _remotelmAgent.Count()) _remoteIndex = 0;
                    if (ex.Message.Contains("model_decommissioned"))
                    {
                        _remoteNextAvailable[currentIndex] = DateTimeOffset.UtcNow.AddDays(365);
                    }
                    if (ex.Message.Contains("Rate limit reached"))
                    {
                        var start = ex.Message.IndexOf("Please try again in");
                        if (start > 0)
                        {
                            start += "Please try again in".Length;
                            var end = ex.Message.IndexOf("s", start);
                            var duration = ex.Message.Substring(start, end - start + 1).Trim();
                            var timespan = TimeSpanParser.Parse(duration);
                            _remoteNextAvailable[currentIndex] = DateTimeOffset.UtcNow.Add(timespan);
                        }
                        else
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
                            Console.WriteLineRed($"Rate limit reached, using local, count {_remoteCount}");
                            _useLocalCount = 5;
                            _remoteCount = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Object reference not set to an instance of an object. - message too long for groq?
                    Console.WriteLineRed(ex.Message);
                    if (_useLocalCount > 0) return null;
                    _useLocalCount++;
                }
            }
            return null;
        }


    }
}
