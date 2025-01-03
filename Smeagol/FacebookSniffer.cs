// Ignore Spelling: Facebook

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Smeagol.Services;
using OpenQA.Selenium.Interactions;
using Gluten.Core.DataProcessing.Service;

namespace Smeagol;

/// <summary>
/// Open and listen for facebook api responses and capture them in a text file for later processing
/// </summary>
internal class FacebookSniffer
{
    private readonly string _responsefileName = "D:\\Coding\\Gluten\\Database\\Responses.txt";
    private readonly ChromeDriver _driver;
    private int _index = 0;

    private readonly DataService _dataService;
    private readonly SettingValues _settings;
    private readonly FBGroupService _fBGroupService;
    private readonly ProcessedGroupPostService _processedGroupPostService;

    private int _newItemCounter = 0;
    private int _skippedItemCounter = 0;

    public FacebookSniffer(DataService dataService, SettingValues settings,
        FBGroupService fBGroupService, ProcessedGroupPostService processedGroupPostService)
    {
        _dataService = dataService;
        _settings = settings;
        _fBGroupService = fBGroupService;
        _processedGroupPostService = processedGroupPostService;
        // JVM options, seems to be better on default
        //var options = new ChromeOptions();
        //options.AddArguments("--js-flags=\" --max_old_space_size=1024 --max_semi_space_size=1024 \"");
        _driver = new ChromeDriver();
    }

    public void Start()
    {
        _dataService.ReadFileLineByLine(_responsefileName);

        var devTools = (OpenQA.Selenium.DevTools.IDevTools)_driver;

        _driver.Manage().Network.NetworkResponseReceived += Network_NetworkResponseReceived;
        var network = _driver.Manage().Network;
        network.StartMonitoring();

        _driver.Navigate().GoToUrl("https://www.facebook.com/");
        //Get the Web Element corresponding to the field Business Email (Text field)
        var email = _driver.FindElement(By.Id("email"));
        var password = _driver.FindElement(By.Id("email"));

        Actions action = new(_driver);

        email.SendKeys(_settings.Email);

        var groups = _fBGroupService.GetKnownGroups();
        Console.WriteLine($"Waiting for login, press a key to continue");
        Console.ReadKey();
        bool multiGroupMode;
        multiGroupMode = true;

        foreach (var item in groups)
        {
            if (item.Value == "NA") continue;
            if (item.Key != "250643821964381") continue; // For debugging purposes
            if (multiGroupMode)
            {
                _driver.Navigate().Refresh();
                _driver.Navigate().GoToUrl($"https://www.facebook.com/groups/{item.Key}/?sorting_setting=CHRONOLOGICAL");
                Console.WriteLine($"Selecting group {item.Key}");
            }
            bool keepRunning = true;
            try
            {
                while (keepRunning)
                {
                    _skippedItemCounter = 0;
                    _newItemCounter = 0;

                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine($"Sending page down {i}");
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        Thread.Sleep(1000 * 3);
                    }

                    if (_newItemCounter == 0 && multiGroupMode)
                    {
                        keepRunning = false;
                        break;
                    }

                    for (int i = 0; i < 40; i++)
                    {
                        Console.WriteLine($"Sending page down {i}");
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        action.SendKeys(Keys.PageDown).Build().Perform();
                        Thread.Sleep(1000 * 3);
                    }

                    Console.WriteLine($"Pausing");
                    Thread.Sleep(1000 * 60);
                    _processedGroupPostService.SaveGroupPost();

                    // Trim DOM
                    for (int i = 0; i < 500; i++)
                    {
                        _driver.ExecuteScript("if (document.querySelectorAll('[role=\"feed\"]')[0].children.length>40){ console.log('clearing'); document.querySelectorAll('[role=\"feed\"]')[0].removeChild(document.querySelectorAll('[role=\"feed\"]')[0].children[0])};");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine($"Complete ");
    }

    /// <summary>
    /// Try to shut down the driver
    /// </summary>
    public void Close()
    {
        try
        {
            _driver.ClearNetworkConditions();
        }
        catch
        {

        }
        _driver.CloseDevToolsSession();
        try
        {
            _driver.Close();
        }
        catch
        {

        }
    }

    private void Network_NetworkResponseReceived(object? sender, NetworkResponseReceivedEventArgs e)
    {
        if (e.ResponseUrl.Equals("https://www.facebook.com/api/graphql/", StringComparison.CurrentCultureIgnoreCase))
        {

            _index++;
            try
            {
                if (!_dataService.LoadLine(e.ResponseBody))
                {
                    System.IO.File.AppendAllText(_responsefileName, e.ResponseBody + "\r\n");
                    Console.WriteLine($"Response saved {_index}");
                    _newItemCounter++;
                }
                else
                {
                    Console.WriteLine($"Response skipped {_index}");
                    _skippedItemCounter++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
