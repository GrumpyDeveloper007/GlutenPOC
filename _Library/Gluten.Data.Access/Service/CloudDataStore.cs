using Gluten.Data.Access.DatabaseModel;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Gluten.Data.Access.Service
{
    /// <summary>
    /// Provides generic access to the cloud DB
    /// </summary>
    public class CloudDataStore : IDisposable
    {
        private const string DatabaseId = "gluencosmos";
        private readonly DbMapper _mappingService = new();
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Dictionary<string, Container> _containers = [];

        /// <summary>
        /// Constructor
        /// </summary>
        public CloudDataStore(string EndpointUri, string PrimaryKey)
        {
            _cosmosClient = new(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "TheShire" });
            _database = _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId).Result;
        }

        /// <summary>
        /// Get all objects of a specific type
        /// </summary>
        public async Task<List<returnType>> GetData<dbObject, returnType>() where dbObject : IDbModel, new()
        {
            return await GetData<dbObject, returnType>("");
        }

        /// <summary>
        /// Delete all data
        /// </summary>
        public async Task DeleteContainer<dbObject>() where dbObject : IDbModel, new()
        {
            var container = await GetContainer<dbObject>(new dbObject());
            await container.DeleteContainerAsync();
            _containers.Remove(new dbObject().GetContainerId());
        }

        /// <summary>
        /// Get data object based on a given filter
        /// </summary>
        public async Task<List<returnType>> GetData<dbObject, returnType>(string whereClause) where dbObject : IDbModel, new()
        {
            var container = await GetContainer<dbObject>(new dbObject());
            var sqlQueryText = $"SELECT * FROM c {whereClause}";

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = container.GetItemQueryIterator<dbObject>(queryDefinition);

            var results = new List<returnType>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var PinTopicDb in currentResultSet)
                {
                    var item = _mappingService.Map<returnType, dbObject>(PinTopicDb);
                    results.Add(item);
                }
            }
            return results;
        }

        /// <summary>
        /// Gets database data object with search criteria
        /// </summary>
        public async Task<List<dbObject>> GetData<dbObject>(string whereClause) where dbObject : IDbModel, new()
        {
            var container = await GetContainer<dbObject>(new dbObject());
            var sqlQueryText = $"SELECT * FROM c {whereClause}";

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = container.GetItemQueryIterator<dbObject>(queryDefinition);

            var results = new List<dbObject>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var dbItem in currentResultSet)
                {
                    results.Add(dbItem);
                }
            }
            return results;
        }

        /// <summary>
        /// Deletes an data item
        /// </summary>
        public async Task DeleteItemAsync<dbObject>(dbObject newDbItem) where dbObject : IDbModel, new()
        {
            var container = await GetContainer<dbObject>(new dbObject());
            await container.DeleteItemAsync<dbObject>(newDbItem.Id, new PartitionKey(newDbItem.PartitionKey));
        }

        //public async Task DeletePartitionAsync<dbObject>(dbObject newDbItem) where dbObject : IDbModel, new()
        //{
        //    var container = await GetContainer<dbObject>(new dbObject());
        //    await container.DeleteAllItemsByPartitionKeyStreamAsync(new PartitionKey(newDbItem.PartitionKey));
        //}


        /// <summary>
        /// Update data object
        /// </summary>
        public async Task ReplaceItemAsync<dbObject>(dbObject newDbItem) where dbObject : IDbModel, new()
        {
            var container = await GetContainer<dbObject>(newDbItem);

            try
            {
                await container.UpsertItemAsync(newDbItem);
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public async Task ScaleContainerAsync<dbObject>(int newThroughput) where dbObject : IDbModel, new()
        {
            var container = await GetContainer<dbObject>(new dbObject());
            // Read the current throughput
            try
            {
                int? throughput = await container.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    // Update throughput
                    await container.ReplaceThroughputAsync(newThroughput);
                    Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
                }
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Cannot read container throuthput.");
                Console.WriteLine(cosmosException.ResponseBody);
            }
        }


        /*

        private async Task DeletePinTopicDbItemAsync()
        {
            var container = await GetContainer(new PinTopicDb());
            var partitionKeyValue = "Wakefield";
            var PinTopicDbId = "Wakefield.7";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<PinTopicDb> wakefieldPinTopicDbResponse = await container.DeleteItemAsync<PinTopicDb>(PinTopicDbId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted PinTopicDb [{0},{1}]\n", partitionKeyValue, PinTopicDbId);
        }

        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await _database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["PinTopicDbDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", DatabaseId);

            //Dispose of CosmosClient
            _cosmosClient.Dispose();
        }
        */

        /// <summary>
        /// Close database
        /// </summary>
        public void Dispose()
        {
            _cosmosClient.Dispose();
        }

        private async Task<Container> GetContainer<dbModel>(dbModel model) where dbModel : IDbModel
        {
            if (!_containers.TryGetValue(model.GetContainerId(), out var container))
            {
                container = await _database.CreateContainerIfNotExistsAsync(model.GetContainerId(), "/partitionKey");
                _containers.Add(model.GetContainerId(), container);
            }
            return container;
        }

    }
}
