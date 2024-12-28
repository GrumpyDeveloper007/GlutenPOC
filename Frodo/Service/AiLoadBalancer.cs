// Ignore Spelling: Api Llm

using OpenAI;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using System.ClientModel;
using Gluten.Core.Interface;
using TimeSpanParserUtil;
using System;

namespace Frodo.Service
{
    /// <summary>
    /// Provide a load balanced interface to remote/local AI models
    /// </summary>
    internal class AiLoadBalancer
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _lmAgent;
        private readonly List<MiddlewareStreamingAgent<OpenAIChatAgent>> _remotelmAgent = new List<MiddlewareStreamingAgent<OpenAIChatAgent>>();
        private readonly List<DateTimeOffset> _remoteNextAvailable = new List<DateTimeOffset>();
        private readonly List<string> _clientname = new List<string>();

        private int _useLocalCount = 0;
        private int _remoteIndex = 0;
        private int _remoteCount = 0;
        private readonly IConsole Console;

        public const int GoodRemoteAgents = 6;
        public const int AllAgents = -2;
        public const int LocalAgent = 0;

        /// <summary>
        /// Opens the connection to our local AI
        /// </summary>
        /// 
        public AiLoadBalancer(string grokApiKey, IConsole console)
        {
            Console = console;

            // per day / per minute
            _clientname.Add("llama-3.1-8b-instant"); //                  14,400	20,000	
            _clientname.Add("llama3-8b-8192");//                         14,400	30,000
            _clientname.Add("llama-3.3-70b-versatile");//                14,400	6,000
            _clientname.Add("llama-3.1-70b-versatile");//                  14,400/6,000
            _clientname.Add("llama-3.3-70b-specdec");            //	30	1,000	6,000	100,000
            _clientname.Add("gemma2-9b-it");                     //	30	14,400	15,000	500,000

            _clientname.Add("llama3-8b-8192");                   //	30	14,400	30,000	500,000
            _clientname.Add("llama3-70b-8192");	//                      14,400	6,000
            _clientname.Add("llama3-groq-8b-8192-tool-use-preview");//   14,400	15,000	
            _clientname.Add("llama3-groq-70b-8192-tool-use-preview");//  14,400	15,000
            _clientname.Add("mixtral-8x7b-32768");//                     14,400	5,000
            _clientname.Add("llama-3.2-11b-vision-preview");     //	30	7,000	7,000	500,000
            _clientname.Add("llama-3.2-1b-preview");             //	30	7,000	7,000	500,000
            _clientname.Add("llama-3.2-3b-preview");             //	30	7,000	7,000	500,000
            //var client9name = "gemma-7b-it";                       //	30	14,400	15,000	500,000 - model_decommissioned
            //var client11name = "llama-3.2-11b-text-preview";       //	30	7,000	7,000	500,000 - model_decommissioned
            //var client15name = "llama-3.2-90b-text-preview";       //	30	7,000	7,000	500,000 - model_decommissioned
            //var client14name = "llama-guard-3-8b";                 //	30	14,400	15,000	500,000 - returns only safe/unsafe
            //llama-3.2-11b-text-preview
            //var client19name = "llava-v1.5-7b-4096-preview";         //	30	14,400	30,000	(No limit)
            //llama-3.2-90b-vision-preview	15	3,500	7,000	250,000
            //var client2name = "llama-3.3-70b-specdec";//                    1,000/6,000

            var endpoint = "https://api.groq.com/openai";
            var credential = new ApiKeyCredential(grokApiKey);
            var openaiClient = new OpenAIClient(credential, new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
                NetworkTimeout = new TimeSpan(0, 2, 0)
            });

            for (int i = 0; i < _clientname.Count; i++)
            {
                string? item = _clientname[i];
                _remotelmAgent.Add(new OpenAIChatAgent(
                    chatClient: openaiClient.GetChatClient(_clientname[i]),
                    name: "assistant")
                    .RegisterMessageConnector());
                _remoteNextAvailable.Add(new DateTimeOffset());
                //.RegisterPrintMessage();
            }

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
        /// Sends a message to the AI LLM, prefers remote models (as they are quicker and free)
        /// </summary>
        public async Task<IMessage?> SendLBMessage(string messageText, bool showLlmName, int maxAgent)
        {
            bool retry = true;
            if (maxAgent == AllAgents) maxAgent = _remotelmAgent.Count();
            if (_remoteIndex >= maxAgent) _remoteIndex = 0;
            while (retry)
            {
                var currentIndex = _remoteIndex;
                try
                {
                    if (_useLocalCount > 0 || maxAgent == LocalAgent)
                    {
                        _remoteCount = 0;

                        if (showLlmName && maxAgent != LocalAgent) Console.WriteLineBlue($"local");
                        IMessage? response = await _lmAgent.SendAsync(messageText);
                        _useLocalCount--;
                        return response;
                    }
                    else
                    {
                        bool allUnavailable = true;
                        for (int i = 0; i < maxAgent; i++)
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
                            if (_remoteIndex >= maxAgent) _remoteIndex = 0;
                            continue;
                        }

                        _remoteCount++;
                        IMessage? response;
                        if (showLlmName) Console.WriteLineBlue($"agent : {_remoteIndex} : {_clientname[_remoteIndex]}");
                        response = await _remotelmAgent[_remoteIndex].SendAsync(messageText);
                        _remoteIndex++;
                        if (_remoteIndex >= maxAgent) _remoteIndex = 0;
                        return response;
                    }
                }
                catch (ClientResultException ex)
                {
                    _remoteIndex++;
                    if (_remoteIndex == maxAgent) _remoteIndex = 0;
                    if (_useLocalCount > 0 || maxAgent == LocalAgent)
                    {
                        retry = false;
                    }
                    if (ex.Message.Contains("Request too large for model"))
                    {
                        _useLocalCount = 1;
                    }
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
