// See https://aka.ms/new-console-template for more information
using Frodo;
using Frodo.Service;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json")
    .AddEnvironmentVariables()
    .Build();

var settings = config.GetRequiredSection("Values").Get<SettingValues>();

var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);

var service = new DataSyncService(nlp);
service.ProcessFile();
