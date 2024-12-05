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


        public void SaveGroupPost()
        {
            JsonHelper.SaveDb<List<string>>(GroupPostProcessedFileName, _LoadedIds);
        }

        public List<string> LoadGroupPost()
        {
            var data = JsonHelper.TryLoadJson<string>(GroupPostProcessedFileName);
            if (data == null) return new List<string>();
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
            while ((line = sr.ReadLine()) != null)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Loading data store {i}");
                }
                if (line != null)
                {
                    var messages = line.Split(crlf, StringSplitOptions.None);

                    var duplicatedLine = true;
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
                            Console.WriteLine(message);
                        }

                    }
                    if (duplicatedLine)
                    {
                        Console.WriteLine($"Duplicate key in data store {i}");
                    }
                }
                i++;
            }

            Console.WriteLine($"Nodes loaded {_LoadedIds.Count}");
        }

        /// <summary>
        /// Returns true if DB already contains this nodeId
        /// </summary>
        public bool LoadLine(string line)
        {
            var duplicatedLine = true;
            var messages = line.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (var message in messages)
            {
                if (message.StartsWith("{\"data\":")) continue;
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
