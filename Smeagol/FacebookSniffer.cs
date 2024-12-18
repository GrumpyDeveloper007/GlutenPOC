// Ignore Spelling: Facebook

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Smeagol.Services;

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

    public FacebookSniffer(DataService dataService, SettingValues settings)
    {
        _dataService = dataService;
        _settings = settings;
        var options = new ChromeOptions();
        options.AddArguments("--js-flags=\" --max_old_space_size=1024 --max_semi_space_size=1024 \"");
        _driver = new ChromeDriver(options);
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

        email.SendKeys(_settings.Email);
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
        _driver.Close();
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
                }
                else
                {
                    Console.WriteLine($"Response skipped {_index}");
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
