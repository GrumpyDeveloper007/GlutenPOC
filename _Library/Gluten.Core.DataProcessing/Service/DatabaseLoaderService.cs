using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.MapsModel;
using Gluten.Data.PinCache;
using Gluten.Data.PinDescription;
using Gluten.Data.TopicModel;
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
        private readonly string PinCacheDBFileName = "D:\\Coding\\Gluten\\Database\\pinCache.json";
        private readonly string ExportDBFileName = "D:\\Coding\\Gluten\\Database\\TopicsExport.json";
        private readonly string GMPinExportDBFileName = "D:\\Coding\\Gluten\\Database\\GMPinExport.json";
        private readonly string RestaurantsFileName = "D:\\Coding\\Gluten\\Database\\Restaurant.txt";
        private readonly string PinDescriptionCacheFileName = "D:\\Coding\\Gluten\\Database\\PinDescriptionCache.json";
        private readonly string GMPinFileName = "D:\\Coding\\Gluten\\Database\\GMPin.json";
        private const string PlacenameSkipListFileName = "D:\\Coding\\Gluten\\Database\\PlaceNameSkipList.json";

        private readonly List<PinDescriptionCache> _pinDescriptionsCache;
        private readonly MapPinCache _mapPinCache;

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseLoaderService()
        {
            string json;
            Dictionary<string, TopicPinCache>? pins = null;
            if (File.Exists(PinCacheDBFileName))
            {
                json = File.ReadAllText(PinCacheDBFileName);
                pins = JsonConvert.DeserializeObject<Dictionary<string, TopicPinCache>>(json);
            }
            if (pins != null)
            {
                _mapPinCache = new MapPinCache(pins, new DummyConsole());
            }
            else
            {
                _mapPinCache = new MapPinCache([], new DummyConsole());
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

        /// <summary>
        /// Get Pin Description cache
        /// </summary>
        public PinDescriptionCache? GetPinDescriptionCache(double geoLongitude, double geoLatitude)
        {
            foreach (var item in _pinDescriptionsCache)
            {
                if (item.GeoLatitude == geoLatitude && item.GeoLongitude == geoLongitude)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Add item to the pin description cache
        /// </summary>
        public void AddPinDescriptionCache(string description, double geoLongitude, double geoLatitude)
        {
            var item = GetPinDescriptionCache(geoLongitude, geoLatitude);
            if (item == null)
            {
                _pinDescriptionsCache.Add(new PinDescriptionCache
                {
                    GeoLongitude = geoLongitude,
                    GeoLatitude = geoLatitude,
                    Description = description
                });
            }
            else
            {
                item.Description = description;
            }
        }

        /// <summary>
        /// Save pin description cache
        /// </summary>
        public void SavePinDescriptionCache()
        {
            SaveDb(PinDescriptionCacheFileName, _pinDescriptionsCache);
        }

        /// <summary>
        /// Save restaurant list
        /// </summary>
        public void SaveRestaurantList(List<string> restaurants)
        {
            //eg. 'Train station',
            string fileText = "";
            foreach (var item in restaurants)
            {
                fileText += $"'{item}',\r\n";
            }
            File.WriteAllText(RestaurantsFileName, fileText);
            File.WriteAllText(RestaurantsFileName + ".txt", fileText.Replace("'", "\""));
        }

        /// <summary>
        /// Save AiVenue items that should not be added to future topics
        /// </summary>
        public void SavePlaceSkipList(List<AiVenue> data)
        {
            SaveDb(PlacenameSkipListFileName, data);
        }

        /// <summary>
        /// Load AiVenue skip list
        /// </summary>
        public List<AiVenue> LoadPlaceSkipList()
        {
            var data = TryLoadJson<AiVenue>(PlacenameSkipListFileName);
            if (data == null) return [];
            return data;
        }

        /// <summary>
        /// Save pins generated from Google maps
        /// </summary>
        public void SaveGMPins(List<GMapsPin> data)
        {
            SaveDb<List<GMapsPin>>(GMPinFileName, data);
        }

        /// <summary>
        /// Load pins generated from google maps
        /// </summary>
        public List<GMapsPin> LoadGMPins()
        {
            var data = TryLoadJson<GMapsPin>(GMPinFileName);
            if (data == null) return [];
            return data;
        }
        /// <summary>
        /// Gets the PinHelper
        /// </summary>
        public MapPinCache GetPinCache()
        {
            return _mapPinCache;
        }

        /// <summary>
        /// Saves the pin caches file
        /// </summary>
        public void SavePinDB()
        {
            var pinCache = _mapPinCache.GetCache();
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
            pins ??= [];
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
