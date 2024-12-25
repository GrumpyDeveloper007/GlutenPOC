using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Helper
{
    public static class SmartPlaceNameFilterHelper
    {
        public static bool IsSkippable(string placeName)
        {
            if (placeName.StartsWith("The seafood", StringComparison.InvariantCulture)) return true;

            return false;
        }
    }
}
