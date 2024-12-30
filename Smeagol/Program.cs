using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Microsoft.Extensions.Configuration;
using Smeagol;
using Smeagol.Services;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json")
    .AddEnvironmentVariables()
    .Build();

var settings = config.GetRequiredSection("Values").Get<SettingValues>();

var data = new DataService();
var group = new FBGroupService(new DummyConsole());
Console.WriteLine("Loading...");

var sniffer = new FacebookSniffer(data, settings, group);
sniffer.Start();
Console.WriteLine("Listening for DB group API responses.");

Console.WriteLine("Press the anykey to exit.");
Console.ReadKey();
data.SaveGroupPost();

sniffer.Close();



