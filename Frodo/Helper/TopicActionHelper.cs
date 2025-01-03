using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Helper
{
    /// <summary>
    /// Topic actions helper class
    /// </summary>
    internal static class TopicActionHelper
    {
        /// <summary>
        /// Remove and reset chain generated
        /// </summary>
        public static int RemoveChainGeneratedAiPins(DetailedTopic? topic)
        {
            int actionCount = 0;
            if (topic == null || topic.AiVenues == null) return actionCount;
            for (int t = topic.AiVenues.Count - 1; t >= 0; t--)
            {
                var venue = topic.AiVenues[t];
                if (venue.IsChain) venue.PinSearchDone = false;

                if (topic.AiVenues[t].ChainGenerated)
                {
                    topic.AiVenues.RemoveAt(t);
                    actionCount++;
                }
            }
            return actionCount;
        }
    }
}
