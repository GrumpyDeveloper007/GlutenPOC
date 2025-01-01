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
    /// <summary>
    /// Extracts key information from FB data
    /// </summary>
    internal class DataService(ProcessedGroupPostService _processedGroupPostService)
    {
        internal static readonly string[] crlf = ["/r/n"];
        internal static readonly string[] separator = ["\r\n"];

        /// <summary>
        /// Read any previously processed data to make sure its in the DB
        /// </summary>
        public void ReadFileLineByLine(string filePath)
        {
            _processedGroupPostService.LoadGroupPost();

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
                                var m = JsonConvert.DeserializeObject<SimplifiedGroupRoot>(message);
                                var nodeId = FbModelHelper.GetNodeId(m);
                                duplicatedLine = _processedGroupPostService.TryAddId(nodeId);
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                Console.WriteLine($"error count : {errorCount}, {ex.Message}");
                            }

                        }
                        if (duplicatedLine)
                        {
                            //Console.WriteLine($"Duplicate key in data store {i}");
                        }
                    }
                }
                i++;
            }

            Console.WriteLine($"Nodes loaded {_processedGroupPostService.NodeCount()}");
        }

        /// <summary>
        /// Returns true if DB already contains this nodeId
        /// </summary>
        public bool LoadLine(string line)
        {
            var duplicatedLine = true;

            var messages = line.Split(separator, StringSplitOptions.None);
            foreach (var message in messages)
            {
                if (message.StartsWith("{\"data\":"))
                {
                    if (!LoadSearchRootMessage(message))
                    {
                        duplicatedLine = false;
                    }
                }

                SimpleGroupRoot gr;
                gr = JsonConvert.DeserializeObject<SimpleGroupRoot>(message) ?? new();
                if (gr != null && gr.label != null && gr.label.Contains("GroupsCometFeedRegularStories"))
                {
                    //"GroupsCometFeedRegularStories_paginationGroup$defer$GroupsCometFeedRegularStories_group_group_feed$page_info
                    try
                    {
                        var m = JsonConvert.DeserializeObject<SimplifiedGroupRoot>(message);
                        var nodeId = FbModelHelper.GetNodeId(m);
                        if (nodeId == null)
                        {
                            //Console.WriteLine($"Unknown Node Id, {m.label}");
                        }
                        else
                        {
                            //Console.WriteLine($"known Node Id,   {m.label}, {nodeId}");
                        }
                        duplicatedLine = _processedGroupPostService.TryAddId(nodeId);
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

        private bool LoadSearchRootMessage(string message)
        {
            var duplicatedLine = true;
            var sr = JsonConvert.DeserializeObject<SearchRoot>(message);
            if (sr?.data?.serpResponse == null) return duplicatedLine;
            var nodeIds = FbModelHelper.GetNodeIds(sr);
            if (nodeIds == null) return duplicatedLine;
            foreach (var nodeId in nodeIds)
            {
                duplicatedLine = _processedGroupPostService.TryAddId(nodeId);
            }
            return duplicatedLine;
        }

    }
}
