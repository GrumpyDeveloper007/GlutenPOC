using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium;
using System.Diagnostics;
using Smeagol.Services;

namespace Smeagol;

/// <summary>
/// Open and listen for facebook api responses and capture them in a text file for later processing
/// </summary>
internal class FacebookSniffer
{
    private string _responsefileName = Environment.CurrentDirectory + "/Responses.txt";
    private ChromeDriver _driver;
    private DataService _dataService;

    public FacebookSniffer(DataService dataService)
    {
        _driver = new ChromeDriver();
        _dataService = dataService;
    }

    public void Start()
    {
        _dataService.ReadFileLineByLine(_responsefileName);

        var devTools = (OpenQA.Selenium.DevTools.IDevTools)_driver;

        DevToolsSession session = devTools.GetDevToolsSession();
        session.Domains.Network.EnableNetwork().Wait();
        session.DevToolsEventReceived += Session_DevToolsEventReceived;

        var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();

        _driver.Manage().Network.NetworkResponseReceived += Network_NetworkResponseReceived;
        var network = _driver.Manage().Network;
        network.NetworkRequestSent += Network_NetworkRequestSent;
        network.StartMonitoring();

        _driver.Navigate().GoToUrl("https://www.facebook.com/");
    }

    public void Close()
    {
        _driver.ClearNetworkConditions();
        _driver.CloseDevToolsSession();
        _driver.Close();
    }

    private void Session_DevToolsEventReceived(object? sender, DevToolsEventReceivedEventArgs e)
    {
        if (e.EventName != "webSocketFrameReceived" && e.EventName != "webSocketFrameSent" && e.EventName != "dataReceived")
        {
            //Debug.WriteLine($"Event: {e.EventName} - {e.EventData["request"]?["url"]}");
        }

        if (e.EventName == "responseReceived")
        {
            //Debug.WriteLine($"responseReceived: requestId : {e.EventData["requestId"]} - {e.EventData["response"]?["url"]}");
        }
        if (e.EventName == "loadingFinished")
        {
            //Debug.WriteLine($"requestId : {e.EventData["requestId"]} - {e.EventData["encodedDataLength"]}");
        }


        if (e.EventData["headers"] != null && e.EventData["headers"]["x-fb-friendly-name"]?.ToString() == "GroupsCometFeedRegularStoriesPaginationQuery")
        {
            //Debug.WriteLine($"GroupsCometFeedRegularStoriesPaginationQuery: {e.EventData.ToJsonString()}");
        }

        if (e.EventName == "dataReceived")
        {
            //Debug.WriteLine($"dataReceived : {e.EventData["requestId"]} - {e.EventData["encodedDataLength"]}");
        }
    }

    private void Network_NetworkRequestSent(object? sender, NetworkRequestSentEventArgs e)
    {
        Debug.WriteLine("New Work Response received");
    }

    private void Network_NetworkResponseReceived(object? sender, NetworkResponseReceivedEventArgs e)
    {
        if (e.ResponseUrl.ToLower() == "https://www.facebook.com/api/graphql/".ToLower())
        {
            // TODO: Create a better locking solution or post to a DB
            try
            {
                if (!_dataService.LoadLine(e.ResponseBody))
                {
                    System.IO.File.AppendAllText(_responsefileName, e.ResponseBody + "/r/n");
                    Console.WriteLine("Response saved");
                }
                else
                {
                    Console.WriteLine("Response skipped");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
