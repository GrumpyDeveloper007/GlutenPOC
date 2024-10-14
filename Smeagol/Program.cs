// See https://aka.ms/new-console-template for more information
using Smeagol;

Console.WriteLine("Press the anykey to exit.");

var sniffer = new FacebookSniffer();
sniffer.Start();


Console.ReadKey();

sniffer.Close();



