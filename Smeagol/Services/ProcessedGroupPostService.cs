using Gluten.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smeagol.Services
{
    /// <summary>
    /// Handles the DB for processed Group Posts
    /// </summary>
    internal class ProcessedGroupPostService
    {
        private const string GroupPostProcessedFileName = "D:\\Coding\\Gluten\\Database\\GroupPostProcessed.json";
        private List<string> _LoadedIds = [];

        /// <summary>
        /// Save data to the DB
        /// </summary>
        public void SaveGroupPost()
        {
            Console.WriteLine("Saving previously processed Group Posts");
            JsonHelper.SaveDb<List<string>>(GroupPostProcessedFileName, _LoadedIds);
        }

        /// <summary>
        /// Load data from the DB
        /// </summary>
        public List<string> LoadGroupPost()
        {
            Console.WriteLine("Loading previously processed Group Posts");
            var data = JsonHelper.TryLoadJsonList<string>(GroupPostProcessedFileName);
            data ??= [];
            _LoadedIds = data;
            return data;
        }

        /// <summary>
        /// Tries to add a new node id, if it already exists, it returns false
        /// </summary>
        public bool TryAddId(string? nodeId)
        {
            if (nodeId == null) return false;
            if (_LoadedIds.Contains(nodeId)) return false;
            _LoadedIds.Add(nodeId);
            return true;
        }

        /// <summary>
        /// Gets the current size of the DB
        /// </summary>
        public int NodeCount()
        {
            return _LoadedIds.Count;
        }


    }
}
