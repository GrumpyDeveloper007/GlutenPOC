using Gluten.Data.MapsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Helper
{
    public static class GPinHelper
    {
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
    }
}
