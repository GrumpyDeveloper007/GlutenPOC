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

    public DataService _dataService;
    public SettingValues _settings;
    public FBGroupService _fBGroupService;

    public int _newItemCounter = 0;
    public int _skippedItemCounter = 0;

    public FacebookSniffer(DataService dataService, SettingValues settings, FBGroupService fBGroupService)
    {
        _dataService = dataService;
        _settings = settings;
        _fBGroupService = fBGroupService;
        //var options = new ChromeOptions();
        //options.AddArguments("--js-flags=\" --max_old_space_size=1024 --max_semi_space_size=1024 \"");
        _driver = new ChromeDriver();
    }

    public void Start()
    {
        _dataService.ReadFileLineByLine(_responsefileName);

        var devTools = (OpenQA.Selenium.DevTools.IDevTools)_driver;

        //DevToolsSession session = devTools.GetDevToolsSession();
        //session.Domains.Network.EnableNetwork().Wait();
        //var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();

        _driver.Manage().Network.NetworkResponseReceived += Network_NetworkResponseReceived;
        var network = _driver.Manage().Network;
        network.StartMonitoring();

        _driver.Navigate().GoToUrl("https://www.facebook.com/");
        //Get the Web Element corresponding to the field Business Email (Textfield)
        var email = _driver.FindElement(By.Id("email"));
        var password = _driver.FindElement(By.Id("email"));

        Actions action = new Actions(_driver);

        email.SendKeys(_settings.Email);

        Thread.Sleep(1000 * 60);

        //?sorting_setting=CHRONOLOGICAL
        //page.driver.browser.reload

        var groups = _fBGroupService.GetKnownGroups();
        Console.WriteLine($"Press a key to continue");
        Console.ReadKey();
        bool multiGroupMode;
        multiGroupMode = true;

        foreach (var item in groups)
        {
            if (item.Value == "NA") continue;
            if (multiGroupMode)
            {
                _driver.Navigate().GoToUrl($"https://www.facebook.com/groups/{item.Key}/?sorting_setting=CHRONOLOGICAL");
                Console.WriteLine($"Selecting group {item.Key}");
            }
            bool keepRunning = true;
            while (keepRunning)
            {
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine($"Sending page down");
                    action.SendKeys(Keys.PageDown).Build().Perform();
                    action.SendKeys(Keys.PageDown).Build().Perform();
                    action.SendKeys(Keys.PageDown).Build().Perform();
                    action.SendKeys(Keys.PageDown).Build().Perform();
                    Thread.Sleep(1000 * 3);
                }

                Console.WriteLine($"Pausing");
                Thread.Sleep(1000 * 60);
                _dataService.SaveGroupPost();

                // Trim DOM
                for (int i = 0; i < 500; i++)
                {
                    _driver.ExecuteScript("if (document.querySelectorAll('[role=\"feed\"]')[0].children.length>40){ console.log('clearing'); document.querySelectorAll('[role=\"feed\"]')[0].removeChild(document.querySelectorAll('[role=\"feed\"]')[0].children[0])};");
                }

                if (_skippedItemCounter > 40 && _newItemCounter == 0 && multiGroupMode)
                {
                    keepRunning = false;
                }
                if (_skippedItemCounter == 0 && _newItemCounter == 0 && multiGroupMode)
                {
                    keepRunning = false;
                }
                _skippedItemCounter = 0;
                _newItemCounter = 0;
            }
        }

        Console.WriteLine($"Complete ");
        //password.SendKeys();


    }

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
        if (e.ResponseUrl.ToLower() == "https://www.facebook.com/api/graphql/".ToLower())
        {

            _index++;
            //GC.Collect();
            // TODO: Create a better locking solution or post to a DB
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
                    //System.IO.File.AppendAllText("D:\\Coding\\Gluten\\Database\\RejectedResponses.txt", e.ResponseBody + "\r\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
