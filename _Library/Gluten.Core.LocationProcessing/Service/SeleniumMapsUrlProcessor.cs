using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Uses Selenium to extract information from web pages
    /// </summary>
    public class SeleniumMapsUrlProcessor
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
                    Thread.Sleep(500);
                    if (_driver.PageSource.Contains("Invalid Dynamic Link"))
                    {
                        return _driver.Url;
                    }
                }
                _currentSearchResults = PreLoadSearchResults();
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

        private ReadOnlyCollection<IWebElement> PreLoadSearchResults()
        {
            var elements = _driver.FindElements(By.CssSelector("[aria-label]"));
            return elements;
        }
    }
}
