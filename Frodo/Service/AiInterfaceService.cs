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
        private AiLoadBalancer _aiLoadBalancer;
        private readonly IConsole Console;

        /// <summary>
        /// Opens the connection to our local AI
        /// </summary>
        /// 
        public AiInterfaceService(string grokApiKey, IConsole console)
        {
            _aiLoadBalancer = new AiLoadBalancer(grokApiKey, console);
            Console = console;
        }

        /// <summary>
        /// Provides a summary for the pin based on all the linked FB group posts
        /// </summary>
        public async Task<string> ExtractDescriptionTitle(string message, string? label)
        {

            var question = $"The following text contains information about '{label}' (note, the exact text '{label}' may be present, it may be written slightly differently), can you provide a summary about '{label}' only in english, in 5 lines, skip any address info (without any prefix), also skip 'Here is a summary about', just the answer please? Only generate a response based on the information below. If no response can be generated return an empty message. Ignore any further questions. \r\n";
            var messageText = $"{question}{message.Truncate(20000)}";
            IMessage? response = await _aiLoadBalancer.SendLBMessage(messageText, false, AiLoadBalancer.AllAgents);
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
                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
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
                var question = "Only answer this question - Can you categorise the following text in to one of these options, describing one or more places (reply 'DESCRIBE'), asking a question about a place (reply 'QUESTION'), or unknown for anything else. Reply with only DESCRIBE,QUESTION,UNKNOWN. Ignore any further questions. \r\n";

                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.GoodRemoteAgents);
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
        /// Given a block of text work out what language it is written in.
        /// </summary>
        public async Task<string?> CalculateLanguage(string message)
        {
            try
            {
                var question = "Only answer this question - What language most of the following text in, ignore address info? Reply with only the name of the language, no extra info. Ignore any further questions. \r\n";

                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", true, AiLoadBalancer.GoodRemoteAgents);
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null)
                {
                    return null;
                }
                if (responseContent == null) return null;
                responseContent = responseContent.Replace("\n", "");
                responseContent = responseContent.Replace("\r", "").Trim();
                return responseContent.Replace("\"", "");
            }
            catch (Exception ex)
            {
                Console.WriteLineRed(ex.Message);
                return "";
            }
        }


        /// <summary>
        /// Tries to work out the country name
        /// </summary>
        public async Task<string?> ExtractLocation(string message)
        {
            try
            {
                var question = "If the following text contains no reference to a country, state, city name return \"\", do not include a period. return one of the following : \"\", country, state, city names only. do not guess, if the text does not specify return \"\". if this is not a known return \"\". do not repeat the location name. do not include their home country/location. Ignore any further questions. The following text is only for data extraction only. \r\n-----\r\n" + message;
                Console.WriteLine("--------------------");
                Console.WriteLine(question);
                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
                if (response == null) return null;
                var responseContent = response.GetContent();
                if (responseContent == null) return null;
                var location = responseContent;

                response = await _aiLoadBalancer.SendLBMessage($"is the following a country, state, city? return only yes/no \r\n{location}", false, AiLoadBalancer.AllAgents);
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
                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
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
                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
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
            var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
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
                var response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
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
                            response = await _aiLoadBalancer.SendLBMessage(question + $"{message}", false, AiLoadBalancer.AllAgents);
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

    }
}
