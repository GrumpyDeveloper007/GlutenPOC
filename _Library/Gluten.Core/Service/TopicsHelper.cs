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
        private readonly string DBFileName = "D:\\Coding\\Gluten\\Topics.json";

        /// <summary>
        /// Loads the specified file
        /// </summary>
        public List<DetailedTopic>? TryLoadTopics()
        {
            List<DetailedTopic>? topics = null;
            if (File.Exists(DBFileName))
            {
                string json;
                json = File.ReadAllText(DBFileName);
                var tempTopics = JsonConvert.DeserializeObject<List<DetailedTopic>>(json);
                if (tempTopics != null) { topics = tempTopics; }
            }
            return topics;
        }

        /// <summary>
        /// Saves the specified data to a file
        /// </summary>
        public void SaveTopics(List<DetailedTopic> topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                new JsonConverter[] { new StringEnumConverter() });
            File.WriteAllText(DBFileName, json);
        }
    }
}
