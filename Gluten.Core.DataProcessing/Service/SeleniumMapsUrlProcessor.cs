using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Uses Selenium to extract information from web pages
    /// </summary>
    public class SeleniumMapsUrlProcessor
    {
        private string _responsefileName = Environment.CurrentDirectory + "/Responses.txt";
        private ChromeDriver _driver = new ChromeDriver();
        private bool _started = false;

        /// <summary>
        /// Start Selenium
        /// </summary>
        private void Start()
        {
            var devTools = (IDevTools)_driver;

            DevToolsSession session = devTools.GetDevToolsSession();
            var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();
        }

        /// <summary>
        /// Gos to the specified Url
        /// </summary>
        public void GoToUrl(string url)
        {
            if (!_started) Start();
            _driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Gos to the specified url and waits for it to respond
        /// </summary>
        public string GoAndWaitForUrlChange(string url)
        {
            if (!_started) Start();
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
            if (!_started) Start();
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
            if (!_started) return;
            _driver.ClearNetworkConditions();
            _driver.CloseDevToolsSession();
            _driver.Close();
        }

        /// <summary>
        /// Checks if a url is a valid google maps link, waits for the url to be updated to include the location
        /// </summary>
        public string CheckUrlForMapLinks(string url)
        {
            // https://www.google.com/maps/place/onwa/@34.6785478,135.8161308,17z/data=!3m1!4b1!4m6!3m5!1s0x60013a30562e78d3:0xd712400d34ea1a7b!8m2!3d34.6785478!4d135.8161308!16s%2Fg%2F11f3whn720!5m1!1e4?entry=ttu&g_ep=EgoyMDI0MTAwOS4wIKXMDSoASAFQAw%3D%3D
            //"https://maps.app.goo.gl/jzpnWbttyQVicafZ9?g_st=il"
            //"https://maps.google.com/?q=Giappone,%20%E3%80%92605-0826%20Kyoto,%20Higashiyama%20Ward,%20Masuyacho,%20349-21%20%E3%82%B8%E3%83%93%E3%82%A8%E3%83%BB%E6%B4%BB%E5%B7%9D%E9%AD%9A%E6%96%99%E7%90%86%E3%83%BB%E5%8D%81%E5%89%B2%E6%89%8B%E6%89%93%E3%81%A1%E8%95%8E%E9%BA%A6%E5%87%A6%20%E3%80%8C%E6%94%BF%E5%8F%B3%E8%A1%9B%E9%96%80%E3%80%8D(MASAEMON)&ftid=0x600109a72b6b2469:0xa0a247525adfb00f&entry=gps&lucs=,94224825,94227247,94227248,47071704,47069508,94218641,94203019,47084304,94208458,94208447&g_st=com.google.maps.preview.copy):"
            //"https://www.google.com/maps/reviews/data=!4m8!14m7!1m6!2m5!1sChZDSUhNMG9nS0VJQ0FnSUNqbHRqY1JREAE!2m1!1s0x0:0x48f8753e21338831!3m1!1s2"
            //https://goo.gl/maps/QPEHvcC3svjdtf5G9
            // Link to street view
            //https://maps.google.com/maps/api/staticmap?center=34.6845322%2C135.1840363&amp;zoom=-1&amp;size=900x900&amp;language=en&amp;sensor=false&amp;client=google-maps-frontend&amp;signature=yGPXtu3-Vjroz_DtJZLPyDkVVC8\
            // Collection of pins 
            //https://www.google.com/maps/d/viewer?mid=16xtxMz-iijlDOEl-dlQKEa2-A19nxzND&ll=35.67714795882308,139.72588715&z=12
            //
            if (!url.Contains("https://www.google.com/maps/d/viewer")
                && (url.Contains("www.google.com/maps/")
                || url.Contains("maps.app.goo.gl")
                || url.Contains("maps.google.com")
                || url.Contains("https://goo.gl/maps/"))
                )
            {
                if (!url.Contains("/@"))
                {
                    var newUrl = GoAndWaitForUrlChange(url);
                    newUrl = GetCurrentUrl();
                    newUrl = HttpUtility.UrlDecode(newUrl);
                    int timeout = 10;
                    while (!newUrl.Contains("/@") && timeout > 0)
                    {
                        timeout--;
                        Thread.Sleep(500);
                        newUrl = GetCurrentUrl();
                        newUrl = HttpUtility.UrlDecode(newUrl);
                    }
                    if (timeout == 0 && !newUrl.Contains("/@"))
                    {
                        Console.WriteLine("Timeout waiting for url");
                        newUrl = GetCurrentUrl();
                        newUrl = HttpUtility.UrlDecode(newUrl);
                    }

                    return newUrl;
                }
            }
            return url;

        }

    }
}
