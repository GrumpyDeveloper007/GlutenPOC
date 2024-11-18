using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Service
{
    /// <summary>
    /// Some text (json) based database functions
    /// </summary>
    public class TopicsHelper
    {
        /// <summary>
        /// Loads the specified file
        /// </summary>
        public List<DetailedTopic>? TryLoadTopics(string fileName)
        {
            List<DetailedTopic>? topics = null;
            if (File.Exists(fileName))
            {
                string json;
                json = File.ReadAllText(fileName);
                var tempTopics = JsonConvert.DeserializeObject<List<DetailedTopic>>(json);
                if (tempTopics != null) { topics = tempTopics; }
            }
            return topics;
        }

        /// <summary>
        /// Saves the specified data to a file
        /// </summary>
        public void SaveTopics(string fileName, List<DetailedTopic> topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                new JsonConverter[] { new StringEnumConverter() });
            File.WriteAllText(fileName, json);
        }

        public void SaveTopics<typeToSave>(string fileName, typeToSave topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                new JsonConverter[] { new StringEnumConverter() });
            File.WriteAllText(fileName, json);
        }

        /// <summary>
        /// Exports the information required by the client app to a file
        /// </summary>
        public void ExportTopics(string fileName, List<Topic> topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                new JsonConverter[] { new StringEnumConverter() });
            File.WriteAllText(fileName, json);
        }
    }
}
