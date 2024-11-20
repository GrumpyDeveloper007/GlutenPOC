using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class DatabaseLoaderService
    {
        private readonly string PinCacheDBFileName = "D:\\Coding\\Gluten\\pinCache.json";
        private readonly string ExportDBFileName = "D:\\Coding\\Gluten\\TopicsExport.json";

        private PinHelper _pinHelper;

        public DatabaseLoaderService()
        {
            _pinHelper = LoadPinHelper();
        }

        public PinHelper LoadPinHelper()
        {
            string json;
            json = File.ReadAllText(PinCacheDBFileName);
            var pins = JsonConvert.DeserializeObject<Dictionary<string, TopicPinCache>>(json);
            if (pins != null)
            {
                _pinHelper = new PinHelper(pins);
            }
            return _pinHelper;
        }



        public void SavePinDB()
        {
            var pinCache = _pinHelper.GetCache();
            SaveDb(PinCacheDBFileName, pinCache);
        }

        public List<PinTopic>? LoadPinTopics()
        {
            return TryLoadJson<PinTopic>(ExportDBFileName);
        }

        /// <summary>
        /// Exports the information required by the client app to a file
        /// </summary>
        public void SavePinTopics(List<PinTopic> pins)
        {
            SaveDb<List<PinTopic>>(ExportDBFileName, pins);
        }


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

        public static void SaveDb<typeToSave>(string fileName, typeToSave topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                [new StringEnumConverter()]);
            File.WriteAllText(fileName, json);
        }
    }
}
