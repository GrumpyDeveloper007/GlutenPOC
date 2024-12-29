// See https://aka.ms/new-console-template for more information
using Frodo;
using Frodo.Helper;
using Frodo.Service;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.Access.Service;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json")
    .AddEnvironmentVariables()
    .Build();

var settings = config.GetRequiredSection("Values").Get<SettingValues>();

if (settings == null)
{
    Console.WriteLine("Unable to load local.settings.json!");
    return;
}

var dbLoader = new DatabaseLoaderService();

var consoleLogger = new ColorConsole();
var geoService = new GeoService();
var selenium = new SeleniumMapsUrlProcessor(consoleLogger);
var mapper = new MappingService();
var fbGroupService = new FBGroupService(consoleLogger);
var pinCache = dbLoader.GetPinCache();
var dataStore = new CloudDataStore(settings.DbEndpointUri, settings.DbPrimaryKey);
var cityService = new CityService();
var restaurantTypeService = new RestaurantTypeService();
var topicDataLoader = new TopicsDataLoaderService();
var mapMetaExtractor = new MapsMetaExtractorService(consoleLogger);
var ai = new AiInterfaceService(settings.GroqApiKey, consoleLogger);

DataHelper.Console = consoleLogger;
LabelHelper.Console = consoleLogger;

consoleLogger.WriteLineBlue("Blue");
consoleLogger.WriteLineRed("Red");


var mapPin = new MapPinService(selenium, pinCache, geoService, mapMetaExtractor, consoleLogger);
var dataExporter = new ClientExportFileGenerator(dbLoader, mapper, pinCache, fbGroupService, geoService, dataStore, ai, consoleLogger);
var pinCacheSync = new PinCacheSyncService(mapPin, dbLoader, geoService, pinCache, restaurantTypeService, consoleLogger);
var aiPinService = new AiVenueProcessorService(mapPin, mapper, consoleLogger);
var aiVenueLocationService = new AiVenueLocationService(dbLoader, mapper, geoService, fbGroupService, pinCache, topicDataLoader, aiPinService, consoleLogger);
var aiVenueCleanUpService = new AiVenueCleanUpService(geoService, fbGroupService, pinCache, aiVenueLocationService, topicDataLoader, consoleLogger);
var service = new DataSyncService(mapPin, dbLoader, mapper, dataExporter, geoService, fbGroupService, pinCache, pinCacheSync, cityService, aiVenueLocationService, topicDataLoader, aiVenueCleanUpService, ai, aiPinService, consoleLogger);
await service.ProcessFile();

selenium.Stop();
