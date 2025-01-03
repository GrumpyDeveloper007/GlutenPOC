using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.LocationProcessing.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    /// <summary>
    /// Process the topic text and responses to find maps embedded links
    /// </summary>
    internal class TopicEmbeddedLinkService(DatabaseLoaderService _databaseLoaderService,
        MapPinService _mapPinService,
        FBGroupService _fBGroupService,
        MappingService _mappingService,
        TopicsDataLoaderService _topicsLoaderService
        )
    {
        /// <summary>
        /// Scan detected urls, try to generate pin information
        /// </summary>
        public void UpdateMessageAndResponseUrls(List<DetailedTopic> Topics)
        {
            int mapsLinkCount = 0;
            int mapsCallCount = 0;
            int searchesDone = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                if (i < 137804) continue;
                Console.WriteLine($"Processing {i} of {Topics.Count} updating embedded urls");
                if (searchesDone > 50)
                {
                    _databaseLoaderService.SavePinDB();
                    searchesDone = 0;
                }
                var topic = Topics[i];
                if (string.IsNullOrWhiteSpace(topic.GroupId))
                {
                    Console.WriteLine($"Missing group id : {i}, {topic.Title} ");
                    continue;
                }
                var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";

                // Update url list from title
                var newUrls = TopicExtractionHelper.ExtractUrls(topic.Title);
                if (topic.UrlsV2 == null)
                {
                    topic.UrlsV2 = newUrls;
                }
                else
                {
                    TopicListHelper.CheckForUpdatedUrls(topic, newUrls);
                }

                // Check for links in topics
                for (int t = 0; t < topic.UrlsV2.Count; t++)
                {
                    var url = topic.UrlsV2[t].Url;
                    if (!MapPinHelper.IsMapsUrl(url)) continue;

                    if (topic.UrlsV2[t].Pin == null)
                    {
                        var cachePin = _mapPinService.TryToGenerateMapPin(url, url, groupCountry, "");
                        if (cachePin == null)
                        {
                            searchesDone++;
                            var newUrl = _mapPinService.CheckUrlForMapLinks(url);
                            cachePin = _mapPinService.TryToGenerateMapPin(newUrl, url, groupCountry, "");
                        }
                        if (cachePin != null)
                        {
                            var newPin = _mappingService.Map<TopicPin, TopicPinCache>(cachePin);
                            topic.UrlsV2[t].Pin = newPin;
                        }
                        mapsCallCount++;
                    }

                    mapsLinkCount++;
                }

                // Look for links in the responses
                for (int z = 0; z < topic.ResponsesV2.Count; z++)
                {
                    var message = topic.ResponsesV2[z].Message;
                    if (message == null) continue;

                    var newLinks = TopicExtractionHelper.ExtractUrls(message);
                    if (topic.ResponsesV2[z].Links == null)
                    {
                        topic.ResponsesV2[z].Links = newLinks;
                    }

                    var links = topic.ResponsesV2[z].Links;
                    if (links != null)
                    {
                        for (int t = 0; t < links.Count; t++)
                        {
                            var url = links[t].Url;
                            if (!MapPinHelper.IsMapsUrl(url)) continue;


                            if (links[t].Pin == null)
                            {
                                var cachePin = _mapPinService.TryToGenerateMapPin(url, url, groupCountry, "");
                                if (cachePin == null)
                                {
                                    searchesDone++;
                                    var newUrl = _mapPinService.CheckUrlForMapLinks(url);
                                    cachePin = _mapPinService.TryToGenerateMapPin(newUrl, url, groupCountry, "");
                                }
                                if (cachePin != null)
                                {
                                    var newPin = _mappingService.Map<TopicPin, TopicPinCache>(cachePin);
                                    links[t].Pin = newPin;
                                }
                                mapsCallCount++;
                            }

                            mapsLinkCount++;
                        }
                    }
                }

            }

            _databaseLoaderService.SavePinDB();
            _topicsLoaderService.SaveTopics(Topics);
            Console.WriteLine($"Has Maps Links : {mapsLinkCount}");
        }

    }
}
