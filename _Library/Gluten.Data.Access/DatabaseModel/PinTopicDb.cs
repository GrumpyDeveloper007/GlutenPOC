using Gluten.Data.ClientModel;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.Access.DatabaseModel
{
    public interface IDbModel
    {
        string Id { get; }
        string PartitionKey { get; }
        string GetContainerId();
    }

    /// <summary>
    /// Data structure used in the client application, represents a pin on the map
    /// </summary>
    public class PinTopicDb : PinTopic, IDbModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            { return GetNodeId(); }
        }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey
        {
            get
            { return GetPartitionKey().ToString(); }
        }

        public string GetContainerId()
        {
            return "PinTopic";
        }

        private string GetNodeId()
        {
            return $"{GeoLatitude}:{GeoLongitude}";
        }

        private PartitionKey GetPartitionKey()
        {
            return new PartitionKey(RestaurantType);
        }
    }

}
