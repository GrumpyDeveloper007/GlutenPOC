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
    public class TopicsHelper
    {
        public List<Topic>? TryLoadTopics(string fileName)
        {
            List<Topic>? topics = null;
            if (File.Exists(fileName))
            {
                string json;
                json = File.ReadAllText(fileName);
                var tempTopics = JsonConvert.DeserializeObject<List<Topic>>(json);
                if (tempTopics != null) { topics = tempTopics; }
            }
            return topics;
        }

        public void SaveTopics(string fileName, List<Topic> topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                new JsonConverter[] { new StringEnumConverter() });
            File.WriteAllText(fileName, json);
        }
    }
}
