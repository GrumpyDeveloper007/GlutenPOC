using AutoMapper;
using Gluten.Data.Access.DatabaseModel;
using Gluten.Data.ClientModel;
using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Data.Access.Service
{
    public class DataStore(string EndpointUri, string PrimaryKey) : IDisposable
    {
        private readonly MappingService _mappingService = new();
        private readonly string databaseId = "gluencosmos";
        private readonly CosmosClient _cosmosClient = new(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "TheShire" });
        private Database _database;
        private Container? _container;
        private readonly Dictionary<string, Container> _containers = [];


        public async Task<Container> GetContainer<dbModel>(dbModel model) where dbModel : IDbModel
        {
            if (!_containers.TryGetValue(model.GetContainerId(), out var container))
            {
                container = await _database.CreateContainerIfNotExistsAsync(model.GetContainerId(), "/partitionKey");
                _containers.Add(model.GetContainerId(), container);
            }
            return container;
        }

        public async Task<List<PinTopic>> GetData()
        {
            if (_container == null) await GetContainer<PinTopicDb>(new PinTopicDb());
            var sqlQueryText = "SELECT * FROM c ";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<PinTopicDb>(queryDefinition);

            var results = new List<PinTopic>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var PinTopicDb in currentResultSet)
                {
                    var item = _mappingService.Map<PinTopic, PinTopicDb>(PinTopicDb);
                    results.Add(item);
                    Console.WriteLine("\tRead {0}\n", PinTopicDb);
                }
            }
            return results;
        }

        public async Task ReplacePinTopicDbItemAsync(PinTopicDb newDbItem)
        {
            if (_container == null) await GetContainer<PinTopicDb>(new PinTopicDb());

            try
            {
                await _container.UpsertItemAsync(newDbItem);
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }



        public async Task GetStartedDemoAsync()
        {
            try
            {
                await CreateDatabaseAsync();
                await CreateContainerAsync();
                //await this.ScaleContainerAsync();
                await AddItemsToContainerAsync();
                await QueryItemsAsync();
                await ReplacePinTopicDbItemAsync();
                await DeletePinTopicDbItemAsync();
                //await DeleteDatabaseAndCleanupAsync();
            }
            catch (CosmosException de)
            {
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
        }

        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", _database.Id);
        }

        private async Task CreateContainerAsync()
        {
            _container = await GetContainer(new PinTopicDb());
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        }

        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            try
            {
                int? throughput = await _container.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    int newThroughput = throughput.Value + 100;
                    // Update throughput
                    await _container.ReplaceThroughputAsync(newThroughput);
                    Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
                }
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Cannot read container throuthput.");
                Console.WriteLine(cosmosException.ResponseBody);
            }

        }

        private async Task AddItemToContainerAsync(PinTopic item)
        {
            PinTopicDb newDbItem = _mappingService.Map<PinTopicDb, PinTopic>(item);

            try
            {
                await _container.UpsertItemAsync(newDbItem);
                // Read the item to see if it exists.  
                //var response = await container.ReadItemAsync<PinTopicDb>(newDbItem.Id, new PartitionKey(newDbItem.RestaurantType));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var response = await _container.CreateItemAsync(newDbItem);
                Console.WriteLine($"Created item in database with id: {response.Resource.Id} Operation consumed {response.RequestCharge} RUs.\n");
            }
        }



        private async Task AddItemsToContainerAsync()
        {
            // Create a PinTopicDb object for the Andersen PinTopicDb
            PinTopicDb andersenPinTopicDb = new()
            {
                Label = "Andersen",
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<PinTopicDb> andersenPinTopicDbResponse = await _container.ReadItemAsync<PinTopicDb>(andersenPinTopicDb.Id, new PartitionKey(andersenPinTopicDb.RestaurantType));
                Console.WriteLine("Item in database with id: {0} already exists\n", andersenPinTopicDbResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen PinTopicDb. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<PinTopicDb> andersenPinTopicDbResponse = await _container.CreateItemAsync(andersenPinTopicDb, new PartitionKey(andersenPinTopicDb.RestaurantType));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenPinTopicDbResponse.Resource.Id, andersenPinTopicDbResponse.RequestCharge);
            }

            // Create a PinTopicDb object for the Wakefield PinTopicDb
            PinTopicDb wakefieldPinTopicDb = new()
            {
            };

            try
            {
                // Read the item to see if it exists
                ItemResponse<PinTopicDb> wakefieldPinTopicDbResponse = await _container.ReadItemAsync<PinTopicDb>(wakefieldPinTopicDb.Id, new PartitionKey(wakefieldPinTopicDb.RestaurantType));
                Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldPinTopicDbResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield PinTopicDb. Note we provide the value of the partition key for this item, which is "Wakefield"
                ItemResponse<PinTopicDb> wakefieldPinTopicDbResponse = await _container.CreateItemAsync(wakefieldPinTopicDb, new PartitionKey(wakefieldPinTopicDb.RestaurantType));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldPinTopicDbResponse.Resource.Id, wakefieldPinTopicDbResponse.RequestCharge);
            }
        }

        private async Task QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.PartitionKey = 'Andersen'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<PinTopicDb>(queryDefinition);

            var results = new List<PinTopicDb>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var PinTopicDb in currentResultSet)
                {
                    results.Add(PinTopicDb);
                    Console.WriteLine("\tRead {0}\n", PinTopicDb);
                }
            }
        }

        private async Task ReplacePinTopicDbItemAsync()
        {
            ItemResponse<PinTopicDb> wakefieldPinTopicDbResponse = await _container.ReadItemAsync<PinTopicDb>("Wakefield.7", new PartitionKey("Wakefield"));
            var itemBody = wakefieldPinTopicDbResponse.Resource;

            // replace the item with the updated content
            wakefieldPinTopicDbResponse = await _container.ReplaceItemAsync(itemBody, itemBody.Id, new PartitionKey(itemBody.RestaurantType));
            Console.WriteLine("Updated PinTopicDb [{0},{1}].\n \tBody is now: {2}\n", itemBody.Label, itemBody.Id, wakefieldPinTopicDbResponse.Resource);
        }



        private async Task DeletePinTopicDbItemAsync()
        {
            var partitionKeyValue = "Wakefield";
            var PinTopicDbId = "Wakefield.7";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<PinTopicDb> wakefieldPinTopicDbResponse = await _container.DeleteItemAsync<PinTopicDb>(PinTopicDbId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted PinTopicDb [{0},{1}]\n", partitionKeyValue, PinTopicDbId);
        }

        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await _database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["PinTopicDbDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", databaseId);

            //Dispose of CosmosClient
            _cosmosClient.Dispose();
        }

        public void Dispose()
        {
            _cosmosClient.Dispose();
        }
    }
}
