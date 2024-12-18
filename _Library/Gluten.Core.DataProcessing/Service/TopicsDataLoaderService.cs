using Gluten.Core.Helper;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Some text (json) based database functions
    /// </summary>
    public class TopicsDataLoaderService
    {
        private const string DBFileName = "D:\\Coding\\Gluten\\Database\\Topics.json";

        /// <summary>
        /// Loads the specified file
        /// </summary>
        public List<DetailedTopic>? TryLoadTopics()
        {
            List<DetailedTopic>? topics = null;
            if (File.Exists(DBFileName))
            {
                var tempTopics = JsonHelper.TryLoadJsonList<DetailedTopic>(DBFileName);
                if (tempTopics != null) { topics = tempTopics; }
            }
            return topics;
        }

        /// <summary>
        /// Saves the specified data to a file
        /// </summary>
        public void SaveTopics(List<DetailedTopic> topics)
        {
            JsonHelper.SaveDb(DBFileName, topics);
        }
    }
}
