using Gluten.FBModel;
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
        private readonly List<string> _LoadedIds = [];

        public void ReadFileLineByLine(string filePath)
        {
            if (!File.Exists(filePath)) return;
            if (File.Exists(filePath + "2-")) File.Delete(filePath + "2-");
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
                    var messages = line.Split(new string[] { "}/r/n" }, StringSplitOptions.None);
                    var duplicatedLine = true;
                    foreach (var message in messages)
                    {
                        try
                        {
                            GroupRoot? m;
                            if (messages.Length > 1 && message != messages[messages.Length - 1])
                            {
                                m = JsonConvert.DeserializeObject<GroupRoot>(message + "}");
                            }
                            else
                            {
                                m = JsonConvert.DeserializeObject<GroupRoot>(message);
                            }
                            if (!ProcessModel(m, message))
                            {
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
                    else
                    {
                        //System.IO.File.AppendAllText(filePath + "2-", line + "\r\n");
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
                        if (!ProcessModel(m, message))
                        {
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

        private Node? GetStoryNode(GroupRoot? groupRoot)
        {
            if (groupRoot == null) return null;
            if (groupRoot == null || groupRoot.data.node == null) return null;

            var a = groupRoot.data.node.comet_sections;
            var node = groupRoot.data.node;

            if (node.__typename == "Group")
            {
                if (node.group_feed.edges.Count > 1)
                {
                    Console.WriteLine("multiple edges");
                }
                node = node.group_feed.edges[0].node;
            }
            else if (node.__typename != "Story")
            {
                Console.WriteLine("unknown node");
            }
            return node;
        }


        private bool ProcessModel(GroupRoot? groupRoot, string json)
        {
            if (groupRoot == null) return true;
            if (groupRoot == null || groupRoot.data.node == null) return true;

            var node = GetStoryNode(groupRoot);
            if (node == null) return true;

            var nodeId = node.id;
            if (_LoadedIds.Contains(nodeId)) return true;
            _LoadedIds.Add(nodeId);
            var story = node.comet_sections.content.story;
            var messageText = story?.message?.text;
            return false;
        }
    }
}
