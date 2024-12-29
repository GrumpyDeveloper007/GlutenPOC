using Gluten.Data.MapsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Helper
{
    /// <summary>
    /// Helper functions for GMapPins
    /// </summary>
    public static class GPinHelper
    {
        /// <summary>
        /// Add/Update pin
        /// </summary>
        public static void TryAddPin(List<GMapsPin> pins, GMapsPin newPin)
        {
            var found = false;
            foreach (var pin in pins)
            {
                if (pin.GeoLatitude == newPin.GeoLatitude
                    && pin.GeoLongitude == newPin.GeoLongitude)
                {
                    pin.Comment = newPin.Comment;

                    found = true;
                    break;
                }
            }
            if (!found)
            {
                pins.Add(newPin);
            }
        }

        /// <summary>
        /// Add/Update pin
        /// </summary>
        public static void TryAddPin(Dictionary<string, GMapsPin> pins, GMapsPin newPin)
        {
            if (!pins.TryGetValue(newPin.GeoLatitude + ":" + newPin.GeoLongitude, out var found))
            {
                pins.Add(newPin.GeoLatitude + ":" + newPin.GeoLongitude, newPin);
            }
            else
            {
                found.Comment = newPin.Comment;
                found.RestaurantType = newPin.RestaurantType;
            }
        }

        /// <summary>
        /// Add/Update pin
        /// </summary>
        public static void TryAddPinList(List<GMapsPin> pins, GMapsPin newPin)
        {
            var oldPin = pins.SingleOrDefault(o => o.Label == newPin.Label);
            if (oldPin == null)
            {
                pins.Add(newPin);
            }
            else
            {
                oldPin.Comment = newPin.Comment;
                oldPin.Label = newPin.Label;
            }

        }

    }
}
