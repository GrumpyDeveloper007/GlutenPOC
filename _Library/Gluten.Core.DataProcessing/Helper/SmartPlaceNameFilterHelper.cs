using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Helper
{
    /// <summary>
    /// TODO: find generic patterns to filter out place names
    /// </summary>
    public static class SmartPlaceNameFilterHelper
    {
        /// <summary>
        /// Can this place name be skippped
        /// </summary>
        public static bool IsSkippable(string placeName)
        {
            if (placeName.StartsWith("The seafood", StringComparison.InvariantCulture)) return true;

            return false;
        }
    }
}
