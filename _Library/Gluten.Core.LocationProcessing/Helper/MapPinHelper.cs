using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Helper
{
    /// <summary>
    /// Helper functions for map pins
    /// </summary>
    public static class MapPinHelper
    {
        /// <summary>
        /// Is the url a google maps url?
        /// </summary>
        public static bool IsMapsUrl(string url)
        {
            if (url.StartsWith("https://l.facebook.com/")) return false;
            // Link to street view
            //https://maps.google.com/maps/api/staticmap?center=34.6845322%2C135.1840363&amp;zoom=-1&amp;size=900x900&amp;language=en&amp;sensor=false&amp;client=google-maps-frontend&amp;signature=yGPXtu3-Vjroz_DtJZLPyDkVVC8\
            // Collection of pins 
            //https://www.google.com/maps/d/viewer?mid=16xtxMz-iijlDOEl-dlQKEa2-A19nxzND&ll=35.67714795882308,139.72588715&z=12
            //https://www.google.com/maps/d/viewer
            //https://www.google.com/maps/d/viewer?mid=1lfBP-usA4-plyeytg-x0ahF8O3_4VZAy&ll=35.12206654781785%2C134.59593959999998&z=5

            // Directions URL
            if (url.StartsWith("https://www.google.com/maps/dir/"))
            {
                return false;
            }
            // shared places -
            // https://www.google.com/maps/@37.9277531,-163.3426319,3z/data=!4m2!11m1!2sKTBwHOV8fB4j8oTQVa7ASXYg4ULr_g?entry=ttu&g_ep=EgoyMDI0MTIxMS4wIKXMDSoASAFQAw%3D%3D
            // https://www.google.com/maps/@42.4524323,7.431418,6z/    data=!4m2!11m1!2s2faANeKuRZS5YWbCYxRq1w?entry=ttu&g_ep=EgoyMDI0MTIxMS4wIKXMDSoASAFQAw%3D%3D
            // https://www.google.com/maps/@/data=!4m3!11m2!2sEVNWEmzxRyeNeBXjphJA5A!3e3?utm_source=mstt_0&g_ep=CAESCjExLjEyNS4xMDIYACCY0AMqSCw5NDIxMDgzNiw5NDIxNjQxMyw5NDIxMDE5Miw5NDIxMjQ5Niw5NDIwNzUwNiw5NDIxNzUyMyw5NDIxODY1Myw0NzA4NDM5M0ICR0I%3D
            // https://www.google.com/maps/@13.7631696,100.4621654,11z/data=!4m3!11m2!2sEVNWEmzxRyeNeBXjphJA5A!3e3?entry=ttu&g_ep=EgoyMDI0MTIxMS4wIKXMDSoASAFQAw%3D%3D
            // https://www.google.com/maps/@40.4093081,-119.0829872,6z/data=!4m3!11m2!2sunRgbBwhRPWyGtLq6t7WDg!3e3?entry=ttu&g_ep=EgoyMDI0MTIxMS4wIKXMDSoASAFQAw%3D%3D
            //https://www.google.co.uk/maps/@50.0610032,19.875576,13z/data=!4m3!11m2!2sBrNUOTgzRhWqkenRWkXSjA!3e3?entry=ttu&g_ep=EgoyMDI0MTIxMS4wIKXMDSoASAFQAw%3D%3D
            //https://www.google.com/maps/contrib/

            if (!url.Contains("https://www.google.com/maps/d/viewer")
                && !url.Contains("https://www.google.com/maps/d/edit?")
                && !url.Contains("https://www.google.com/maps/contrib/")
                && (url.Contains("www.google.com/maps/")
                || url.Contains("maps.app.goo.gl")
                || url.Contains("maps.google.com")
                || url.Contains("https://goo.gl/maps/"))
                )
            {
                return true;
            }
            return false;
        }
    }
}
