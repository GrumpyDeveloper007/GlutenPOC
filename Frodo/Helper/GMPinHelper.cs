using Gluten.Data.Access.DatabaseModel;
using Gluten.Data.MapsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Helper
{
    /// <summary>
    /// Helper functions for GMapsPin data class
    /// </summary>
    public class GMPinHelper
    {
        /// <summary>
        /// Get matching database pin if exists
        /// </summary>
        public static GMapsPinDb? FindDbPin(List<GMapsPinDb> dbPins, GMapsPin gMapsPin)
        {
            foreach (var pin in dbPins)
            {
                if (pin.GeoLatitude == gMapsPin.GeoLatitude && pin.GeoLongitude == gMapsPin.GeoLongitude)
                {
                    return pin;
                }
            }
            return null;
        }

    }
}
