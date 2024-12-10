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
    internal static class MapPinHelper
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
            if (!url.Contains("https://www.google.com/maps/d/viewer")
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
