using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Interface;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    /// <summary>
    /// Tries to create AiVenues from topics, venues are created from the title text
    /// </summary>
    internal class AiVenueCreationService(
        TopicsDataLoaderService _topicsLoaderService,
        AiInterfaceService _localAi,
        IConsole Console)
    {
        /// <summary>
        /// Use AI to extract information from unformatted human generated data
        /// </summary>
        public async Task ScanTopicsUseAiToDetectVenueInfo(List<DetailedTopic> Topics)
        {
            bool foundUpdates = false;
            Stopwatch timer = new();
            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                if (i < 119568 && topic.IsAiVenuesSearchDone) continue;
                //if (topic.IsAiVenuesSearchDone) continue;
                if (TopicItemHelper.IsRecipe(topic)) continue;
                if (TopicItemHelper.IsTopicAQuestion(topic)) continue;

                if (i % 100 == 0 && foundUpdates)
                {
                    Console.WriteLineBlue($"Saving topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    foundUpdates = false;
                }
                timer.Restart();
                var venue = await _localAi.ExtractRestaurantNamesFromTitle(topic.Title);
                timer.Stop();
                Console.WriteLine($"Processing topic (new) {i} of {Topics.Count} length :{topic.Title.Length} Time: {timer.Elapsed.TotalSeconds}");
                topic.IsAiVenuesSearchDone = true;
                if (SyncVenues(topic, venue))
                {
                    foundUpdates = true;
                }
            }
            _topicsLoaderService.SaveTopics(Topics);
        }

        /// <summary>
        /// Retry data generation for null pins
        /// </summary>
        public async Task ScanTopicsRegenerateNullPins(List<DetailedTopic> Topics)
        {
            int updateCount = 0;
            Stopwatch timer = new();
            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];
                if (TopicItemHelper.IsRecipe(topic)) continue;
                if (topic.AiVenues == null) continue;
                if (TopicItemHelper.IsTopicAQuestion(topic)) continue;

                var reCheck = false;
                var allNull = true;
                if (topic.AiVenues != null)
                    foreach (var item in topic.AiVenues)
                    {
                        if (item.Pin != null) allNull = false;

                        if (item.Pin == null
                            && !item.IsChain)
                        {
                            reCheck = true;
                        }
                    }
                if (!reCheck) continue;

                if (updateCount > 50)
                {
                    Console.WriteLineBlue($"Saving topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    updateCount = 0;
                }
                timer.Restart();
                var venue = await _localAi.ExtractRestaurantNamesFromTitle(topic.Title);
                timer.Stop();
                Console.WriteLine($"Processing topic (null pin) {i} of {Topics.Count} length :{topic.Title.Length} Time: {timer.Elapsed.TotalSeconds}");
                topic.IsAiVenuesSearchDone = true;

                if (allNull)
                {
                    topic.AiVenues = venue;
                    updateCount++;
                }
                else
                {
                    if (SyncVenues(topic, venue))
                    {
                        updateCount++;
                    }
                }
            }
            _topicsLoaderService.SaveTopics(Topics);
        }

        private bool SyncVenues(DetailedTopic topic, List<AiVenue>? newVenues)
        {
            bool foundUpdates = false;
            if (topic.AiVenues == null && newVenues == null) return false;
            if (topic.AiVenues == null && newVenues != null)
            {
                topic.AiVenues = newVenues;
                for (int i = 0; i < newVenues.Count; i++)
                {
                    Console.WriteLineBlue($"Adding new venue :{newVenues[i].PlaceName}");
                }
                return false;
            }

            if (newVenues == null) return false;
            for (int i = 0; i < newVenues.Count; i++)
            {
                topic.AiVenues ??= [];
                var oldVenue = topic.AiVenues.FirstOrDefault(o => o.PlaceName == newVenues[i].PlaceName);
                if (oldVenue != null)
                {
                    if (string.IsNullOrWhiteSpace(oldVenue.City)) oldVenue.City = newVenues[i].City;
                    foundUpdates = true;
                }
                else
                {
                    topic.AiVenues.Add(newVenues[i]);
                    Console.WriteLineBlue($"Adding new venue :{newVenues[i].PlaceName}");
                    foundUpdates = true;
                }
            }
            return foundUpdates;
        }
    }
}
