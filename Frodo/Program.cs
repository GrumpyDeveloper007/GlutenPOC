// See https://aka.ms/new-console-template for more information
using Frodo;
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

var geoService = new GeoService();
var selenium = new SeleniumMapsUrlProcessor();
var mapper = new MappingService();
var fbGroupService = new FBGroupService();
var pinCache = dbLoader.GetPinCache();
var dataStore = new CloudDataStore(settings.DbEndpointUri, settings.DbPrimaryKey);
var cityService = new CityService();

var ai = new MapPinService(selenium, pinCache, geoService, new MapsMetaExtractorService());
var clientExportFileGenerator = new ClientExportFileGenerator(dbLoader, mapper, pinCache, fbGroupService, geoService, dataStore);
var pinCacheSync = new PinCacheSyncService(ai, dbLoader, geoService, pinCache);
var service = new DataSyncService(ai, dbLoader, mapper, clientExportFileGenerator, geoService, fbGroupService, pinCache, pinCacheSync, cityService);
await service.ProcessFile();

selenium.Stop();
