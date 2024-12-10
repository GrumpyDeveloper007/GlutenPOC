// See https://aka.ms/new-console-template for more information
using Frodo;
using Frodo.Helper;
using Frodo.Service;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.LocationProcessing.Service;
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

DataHelper.Console = consoleLogger;
LabelHelper.Console = consoleLogger;

consoleLogger.WriteLineBlue("Blue");
consoleLogger.WriteLineRed("Red");

var ai = new MapPinService(selenium, pinCache, geoService, new MapsMetaExtractorService(restaurantTypeService, consoleLogger), consoleLogger);
var clientExportFileGenerator = new ClientExportFileGenerator(dbLoader, mapper, pinCache, fbGroupService, geoService, dataStore, consoleLogger);
var pinCacheSync = new PinCacheSyncService(ai, dbLoader, geoService, pinCache, restaurantTypeService, consoleLogger);
var service = new DataSyncService(ai, dbLoader, mapper, clientExportFileGenerator, geoService, fbGroupService, pinCache, pinCacheSync, cityService, consoleLogger);
await service.ProcessFile();

selenium.Stop();
