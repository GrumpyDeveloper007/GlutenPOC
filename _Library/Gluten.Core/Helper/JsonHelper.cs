// Ignore Spelling: Json

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Helper
{
    /// <summary>
    /// Json file IO helper class
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Tries to load a collection of classes from a Json file
        /// </summary>
        public static List<classType>? TryLoadJson<classType>(string fileName)
        {
            List<classType>? topics = null;
            if (File.Exists(fileName))
            {
                string json;
                json = File.ReadAllText(fileName);
                var tempTopics = JsonConvert.DeserializeObject<List<classType>>(json);
                if (tempTopics != null) { topics = tempTopics; }
            }
            return topics;
        }

        /// <summary>
        /// Saves a class structure to a Json file
        /// </summary>
        public static void SaveDb<typeToSave>(string fileName, typeToSave topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                [new StringEnumConverter()]);
            File.WriteAllText(fileName, json);
        }
    }
}
