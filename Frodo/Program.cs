// See https://aka.ms/new-console-template for more information
using Frodo;
using Frodo.Service;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Core.Service;
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

//var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);
var geoService = new GeoService();
var pinHelper = new PinHelper();
var selenium = new SeleniumMapsUrlProcessor();
var mapper = new MappingService();
var fbGroupService = new FBGroupService();
var pinCache = dbLoader.GetPinCache();

var ai = new MapPinService(selenium, pinHelper, pinCache, geoService, new MapsMetaExtractorService());
var clientExportFileGenerator = new ClientExportFileGenerator(dbLoader, mapper, pinCache, fbGroupService, geoService);
var service = new DataSyncService(ai, pinHelper, dbLoader, mapper, clientExportFileGenerator, geoService, fbGroupService, pinCache);
service.ProcessFile();

selenium.Stop();
