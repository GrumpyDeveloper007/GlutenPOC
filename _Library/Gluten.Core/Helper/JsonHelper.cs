// Ignore Spelling: Json

using Newtonsoft.Json;

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
        public static List<classType>? TryLoadJsonList<classType>(string fileName)
        {
            List<classType>? myObject = null;
            if (File.Exists(fileName))
            {
                using StreamReader file = File.OpenText(fileName);
                JsonSerializer serializer = new();
                var tempMyObject = serializer.Deserialize(file, typeof(List<classType>));
                if (tempMyObject != null) { myObject = (List<classType>)tempMyObject; }

                //string json;
                //json = File.ReadAllText(fileName);
                //var tempMyObject = JsonConvert.DeserializeObject<List<classType>>(json);
                //if (tempMyObject != null) { myObject = tempMyObject; }
            }
            return myObject;
        }

        /// <summary>
        /// Tries to load a collection of classes from a Json file
        /// </summary>
        public static Dictionary<string, classType>? TryLoadJsonDictionary<classType>(string fileName)
        {
            Dictionary<string, classType>? myObject = null;
            if (File.Exists(fileName))
            {
                using StreamReader file = File.OpenText(fileName);
                JsonSerializer serializer = new();
                var tempMyObject = serializer.Deserialize(file, typeof(Dictionary<string, classType>));
                if (tempMyObject != null) { myObject = (Dictionary<string, classType>)tempMyObject; }

                //string json;
                //json = File.ReadAllText(fileName);
                //var tempMyObject = JsonConvert.DeserializeObject<List<classType>>(json);
                //if (tempMyObject != null) { myObject = tempMyObject; }
            }
            return myObject;
        }

        /// <summary>
        /// Saves a class structure to a Json file
        /// </summary>

        public static void SaveDb<typeToSave>(string fileName, typeToSave objectToSave)
        {
            using StreamWriter file = File.CreateText(fileName);
            JsonSerializer serializer = new();
            serializer.Formatting = Formatting.Indented; // Optional: makes the JSON readable
            serializer.Serialize(file, objectToSave);
        }

        /// <summary>
        /// Saves a class structure to a Json file - in a space saving format
        /// </summary>
        public static void SaveDbNoPadding<typeToSave>(string fileName, typeToSave objectToSave)
        {
            using StreamWriter file = File.CreateText(fileName);
            JsonSerializer serializer = new();
            serializer.Formatting = Formatting.None;
            serializer.Serialize(file, objectToSave);
        }
    }
}
