using Smeagol;
using Smeagol.Services;


var data = new DataService();
Console.WriteLine("Loading...");

var sniffer = new FacebookSniffer(data);
sniffer.Start();
Console.WriteLine("Listening for DB group API responses.");

Console.WriteLine("Press the anykey to exit.");
Console.ReadKey();

sniffer.Close();



