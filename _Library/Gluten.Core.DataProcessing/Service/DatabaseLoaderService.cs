using Gluten.Core.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.MapsModel;
using Gluten.Data.PinCache;
using Gluten.Data.PinDescription;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Provides load/save functionality for our databases (currently in memory/file based)
    /// </summary>
    public class DatabaseLoaderService
    {
        private readonly string PinCacheDBFileName = "D:\\Coding\\Gluten\\pinCache.json";
        private readonly string ExportDBFileName = "D:\\Coding\\Gluten\\TopicsExport.json";
        private readonly string GMPinExportDBFileName = "D:\\Coding\\Gluten\\GMPinExport.json";
        private readonly string RestaurantsFileName = "D:\\Coding\\Gluten\\Restaurant.txt";
        private readonly string PinDescriptionCacheFileName = "D:\\Coding\\Gluten\\PinDescriptionCache.json";
        private readonly string GMPinFileName = "D:\\Coding\\Gluten\\GMPin.json";

        private readonly PinHelper _pinHelper;
        private readonly List<PinDescriptionCache> _pinDescriptionsCache;

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseLoaderService()
        {
            string json;
            json = File.ReadAllText(PinCacheDBFileName);
            var pins = JsonConvert.DeserializeObject<Dictionary<string, TopicPinCache>>(json);
            if (pins != null)
            {
                _pinHelper = new PinHelper(pins);
            }
            else
            {
                _pinHelper = new PinHelper([]);
            }
            if (File.Exists(PinDescriptionCacheFileName))
            {
                json = File.ReadAllText(PinDescriptionCacheFileName);
                var pdc = JsonConvert.DeserializeObject<List<PinDescriptionCache>>(json);
                if (pdc != null)
                {
                    _pinDescriptionsCache = pdc;
                }

            }
            _pinDescriptionsCache ??= [];
        }

        public PinDescriptionCache? GetPinDescriptionCache(List<string> nodes)
        {
            foreach (var item in _pinDescriptionsCache)
            {
                if (item.Nodes.Count == nodes.Count)
                {
                    var found = true;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (item.Nodes[i] != nodes[i])
                        {
                            found = false;
                        }
                    }
                    if (found == true)
                    {
                        return item;
                    }
                }
            }
            return null;
        }


        public void AddPinDescriptionCache(PinDescriptionCache pinDescriptionsCache)
        {
            foreach (var item in _pinDescriptionsCache)
            {
                if (item.Nodes.Count == pinDescriptionsCache.Nodes.Count)
                {
                    var found = true;
                    for (int i = 0; i < pinDescriptionsCache.Nodes.Count; i++)
                    {
                        if (item.Nodes[i] != pinDescriptionsCache.Nodes[i])
                        {
                            found = false;
                        }
                    }
                    if (found == true)
                    {
                        item.Description = pinDescriptionsCache.Description;
                        return;
                    }
                }
            }

            _pinDescriptionsCache.Add(pinDescriptionsCache);
        }

        public void SavePinDescriptionCache()
        {
            SaveDb(PinDescriptionCacheFileName, _pinDescriptionsCache);
        }

        public void SaveRestaurantList(List<string> restaurants)
        {
            //eg. 'Train station',
            string fileText = "";
            foreach (var item in restaurants)
            {
                fileText += $"'{item}',\r\n";
            }
            File.WriteAllText(RestaurantsFileName, fileText);
        }

        public void SaveGMPins(List<GMapsPin> data)
        {
            SaveDb<List<GMapsPin>>(GMPinFileName, data);
        }

        public List<GMapsPin> LoadGMPins()
        {
            var data = TryLoadJson<GMapsPin>(GMPinFileName);
            if (data == null) return new List<GMapsPin>();
            return data;
        }
        /// <summary>
        /// Gets the PinHelper
        /// </summary>
        public PinHelper GetPinHelper()
        {
            return _pinHelper;
        }

        /// <summary>
        /// Saves the pin caches file
        /// </summary>
        public void SavePinDB()
        {
            var pinCache = _pinHelper.GetCache();
            SaveDb(PinCacheDBFileName, pinCache);
        }

        /// <summary>
        /// Loads the previously generated pin topic export file
        /// </summary>
        public List<PinTopic>? LoadPinTopics()
        {
            return TryLoadJson<PinTopic>(ExportDBFileName);
        }

        /// <summary>
        /// Exports the information required by the client app to a file
        /// </summary>
        public void SavePinTopics(List<PinTopic> pins)
        {
            SaveDb(ExportDBFileName, pins);
        }

        /// <summary>
        /// Loads the previously generated pin topic export file
        /// </summary>
        public List<GMapsPin> LoadGMMapPinExport()
        {
            var pins = TryLoadJson<GMapsPin>(GMPinExportDBFileName);
            pins ??= new List<GMapsPin>();
            return pins;
        }

        /// <summary>
        /// Exports the information required by the client app to a file
        /// </summary>
        public void SaveGMMapPinExport(List<GMapsPin> pins)
        {
            SaveDb(GMPinExportDBFileName, pins);
        }

        private static List<classType>? TryLoadJson<classType>(string fileName)
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

        private static void SaveDb<typeToSave>(string fileName, typeToSave topics)
        {
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented,
                [new StringEnumConverter()]);
            File.WriteAllText(fileName, json);
        }
    }
}
