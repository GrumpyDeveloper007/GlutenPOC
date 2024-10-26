// See https://aka.ms/new-console-template for more information
using Frodo;
using Frodo.Service;
using Gluten.Core.DataProcessing.Service;
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
var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);
var selenium = new SeleniumMapsUrlProcessor();
selenium.Start();

var ai = new AIProcessingService(nlp, selenium);
var service = new DataSyncService(ai, selenium);
service.ProcessFile();

selenium.Stop();
