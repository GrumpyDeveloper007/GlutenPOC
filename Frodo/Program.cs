﻿// See https://aka.ms/new-console-template for more information
using Frodo;
using Frodo.Service;
using Gluten.Core.DataProcessing.Service;
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
var pinHelper = dbLoader.GetPinHelper();
var selenium = new SeleniumMapsUrlProcessor();
var mapper = new MappingService();

var ai = new AIProcessingService(selenium, pinHelper, mapper);
var service = new DataSyncService(ai, selenium, pinHelper, dbLoader);
service.ProcessFile();

selenium.Stop();
