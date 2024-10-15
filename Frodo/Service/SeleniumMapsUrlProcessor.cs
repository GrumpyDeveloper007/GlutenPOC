using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class SeleniumMapsUrlProcessor
    {
        private string _responsefileName = Environment.CurrentDirectory + "/Responses.txt";
        private ChromeDriver _driver = new ChromeDriver();

        public void Start()
        {
            var devTools = (OpenQA.Selenium.DevTools.IDevTools)_driver;

            DevToolsSession session = devTools.GetDevToolsSession();
            var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();

            //var network = _driver.Manage().Network;
            //network.StartMonitoring();

        }

        public void GoToUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public string GoAndWaitForUrlChange(string url)
        {
            try
            {
                var existingUrl = url;
                _driver.Navigate().GoToUrl(url);
                while (existingUrl == _driver.Url)
                {
                    System.Threading.Thread.Sleep(500);
                    if (_driver.PageSource.Contains("Invalid Dynamic Link"))
                    {
                        return _driver.Url;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return _driver.Url;
        }

        public string GetCurrentUrl()
        {
            return _driver.Url;
        }

        public void Close()
        {
            _driver.ClearNetworkConditions();
            _driver.CloseDevToolsSession();
            _driver.Close();
        }
    }
}
