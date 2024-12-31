using Gluten.Core.Interface;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Uses Selenium to extract information from web pages
    /// </summary>
    public class SeleniumService(IConsole Console)
    {
        private readonly ChromeDriver _driver = new();
        private ReadOnlyCollection<IWebElement> _currentSearchResults = new([]);

        /// <summary>
        /// Gos to the specified Url
        /// </summary>
        public void GoToUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Gos to the specified url and waits for it to respond
        /// </summary>
        public string GoAndWaitForUrlChange(string url)
        {
            try
            {
                var existingUrl = url;
                _driver.Navigate().GoToUrl(url);
                while (existingUrl == _driver.Url)
                {
                    Thread.Sleep(200);
                    if (_driver.PageSource.Contains("Invalid Dynamic Link")
                        || _driver.PageSource.Contains("Sorry, something went wrong"))
                    {
                        return _driver.Url;
                    }
                }
                PreLoadSearchResults();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return url;
        }

        /// <summary>
        /// Gets the current Url in the Selenium attached browser
        /// </summary>
        public string GetCurrentUrl()
        {
            try { return _driver.Url; }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        /// <summary>
        /// Stop Selenium
        /// </summary>
        public void Stop()
        {
            _driver.ClearNetworkConditions();
            _driver.CloseDevToolsSession();
            _driver.Close();
        }

        /// <summary>
        /// Get aria-label elements
        /// </summary>
        public ReadOnlyCollection<IWebElement> GetSearchResults()
        {
            PreLoadSearchResults();
            return _currentSearchResults;
        }

        /// <summary>
        /// Get the inner html for the first aria-label element
        /// </summary>
        public string GetFirstLabelInnerHTML()
        {
            var r = GetSearchResults();
            foreach (var item in r)
            {
                var a = item.GetAttribute("aria-label");
                if (a != null)
                {
                    return item.GetAttribute("innerHTML");
                }
            }
            return "";
        }

        /// <summary>
        /// Gets interesting elements and saves for later
        /// </summary>
        public void PreLoadSearchResults()
        {
            var elements = _driver.FindElements(By.CssSelector("[aria-label]"));
            _currentSearchResults = elements;
        }

        /// <summary>
        /// Gets the current page source
        /// </summary>
        public string PageSource()
        {
            return _driver.PageSource;
        }
    }
}
