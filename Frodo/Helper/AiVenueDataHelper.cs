using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Helper
{
    /// <summary>
    /// Helper functions for AiVenue data class
    /// </summary>
    internal static class AiVenueDataHelper
    {
        /// <summary>
        /// Searches for duplicated AiVenues in a list
        /// </summary>
        public static int RemoveDuplicatedVenues(List<AiVenue>? venues)
        {
            int removeCount = 0;
            if (venues == null) return 0;

            for (int t = venues.Count - 1; t >= 0; t--)
            {
                if (DataHelper.IsInList(venues, venues[t], t, true))
                {
                    removeCount++;
                    venues.RemoveAt(t);
                }
            }
            return removeCount;
        }
    }
}
