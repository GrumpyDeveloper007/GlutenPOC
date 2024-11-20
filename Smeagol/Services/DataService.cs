using Newtonsoft.Json;
using Smeagol.FacebookModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smeagol.Services
{
    internal class DataService
    {
        private HashSet<string> _LoadedIds = new HashSet<string>();

        public void ReadFileLineByLine(string filePath)
        {
            if (!File.Exists(filePath)) return;
            // Open the file and read each line
            using (StreamReader sr = new StreamReader(filePath))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        var messages = line.Split(new string[] { "}/r/n" }, StringSplitOptions.None);
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
                                ProcessModel(m);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(message);

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if DB already contains this nodeId
        /// </summary>
        public bool LoadLine(string line)
        {
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
                        if (!ProcessModel(m)) return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(message);
                        Console.WriteLine(ex.Message);
                        return true;
                    }
                }
            }
            return true;
        }

        private bool ProcessModel(GroupRoot? groupRoot)
        {
            if (groupRoot == null) return true;
            string? messageText;
            if (groupRoot == null || groupRoot.data.node == null) return true;

            var a = groupRoot.data.node.comet_sections;
            var nodeId = groupRoot.data.node.id;
            if (_LoadedIds.Contains(nodeId)) return true;
            _LoadedIds.Add(nodeId);
            if (a != null)
            {
                var story = a.content.story;
                messageText = story?.message?.text;
            }
            return false;
        }
    }
}
