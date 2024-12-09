using Gluten.Core.Helper;
using Gluten.FBModel;
using Gluten.FBModel.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smeagol.Services
{
    internal class DataService
    {
        private List<string> _LoadedIds = [];
        internal static readonly string[] crlf = ["/r/n"];
        private const string GroupPostProcessedFileName = "D:\\Coding\\Gluten\\Database\\GroupPostProcessed.json";
        internal static readonly string[] separator = { "\r\n" };

        public void SaveGroupPost()
        {
            JsonHelper.SaveDb<List<string>>(GroupPostProcessedFileName, _LoadedIds);
        }

        public List<string> LoadGroupPost()
        {
            var data = JsonHelper.TryLoadJson<string>(GroupPostProcessedFileName);
            if (data == null) return [];
            return data;
        }

        public void ReadFileLineByLine(string filePath)
        {
            _LoadedIds = LoadGroupPost();

            if (!File.Exists(filePath)) return;
            // Open the file and read each line
            using StreamReader sr = new(filePath);
            string? line;
            int i = 0;
            int errorCount = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Loading data store {i}");
                }
                if (line != null)
                {
                    var duplicatedLine = true;

                    if (line.StartsWith("{\"data\":"))
                    {
                        if (!LoadSearchRootMessage(line))
                        {
                            duplicatedLine = false;
                        }
                    }
                    else
                    {
                        var messages = line.Split(crlf, StringSplitOptions.None);
                        foreach (var message in messages)
                        {
                            if (string.IsNullOrWhiteSpace(message)) continue;
                            try
                            {
                                GroupRoot? m = JsonConvert.DeserializeObject<GroupRoot>(message);
                                var nodeId = FbModelHelper.GetNodeId(m);
                                if (nodeId != null && !_LoadedIds.Contains(nodeId))
                                {
                                    _LoadedIds.Add(nodeId);
                                    duplicatedLine = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                Console.WriteLine($"error count : {errorCount}, {ex.Message}");
                            }

                        }
                        if (duplicatedLine)
                        {
                            Console.WriteLine($"Duplicate key in data store {i}");
                        }
                    }
                }
                i++;
            }

            Console.WriteLine($"Nodes loaded {_LoadedIds.Count}");
        }

        private bool LoadSearchRootMessage(string message)
        {
            var duplicatedLine = true;
            var sr = JsonConvert.DeserializeObject<SearchRoot>(message);
            if (sr?.data?.serpResponse == null) return duplicatedLine;
            var nodeIds = FbModelHelper.GetNodeIds(sr);
            if (nodeIds == null) return duplicatedLine;
            foreach (var nodeId in nodeIds)
            {
                if (nodeId != null && !_LoadedIds.Contains(nodeId))
                {
                    _LoadedIds.Add(nodeId);
                    duplicatedLine = false;
                }
            }
            return duplicatedLine;
        }

        /// <summary>
        /// Returns true if DB already contains this nodeId
        /// </summary>
        public bool LoadLine(string line)
        {
            var duplicatedLine = true;

            if (line.StartsWith("{\"data\":"))
            {
                if (!LoadSearchRootMessage(line))
                {
                    duplicatedLine = false;
                }
                return duplicatedLine;
            }

            var messages = line.Split(separator, StringSplitOptions.None);
            foreach (var message in messages)
            {
                SimpleGroupRoot gr;
                gr = JsonConvert.DeserializeObject<SimpleGroupRoot>(message);
                if (gr != null && gr.label != null && gr.label.Contains("GroupsCometFeedRegularStories"))
                {

                    try
                    {
                        GroupRoot? m;
                        m = JsonConvert.DeserializeObject<GroupRoot>(message);
                        var nodeId = FbModelHelper.GetNodeId(m);
                        if (nodeId == null)
                        {
                            //Console.WriteLine($"Unknown Node Id, {message}");
                        }
                        if (nodeId != null && !_LoadedIds.Contains(nodeId))
                        {
                            _LoadedIds.Add(nodeId);
                            duplicatedLine = false;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(message);
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return duplicatedLine;
        }

    }
}
