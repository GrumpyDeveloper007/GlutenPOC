using Azure.AI.TextAnalytics;
using Gluten.Core.Service;
using Gluten.Data;
using Gluten.Data.TopicModel;
using System.Web;

namespace Gluten.Core.DataProcessing.Service
{
    public class AIProcessingService
    {
        private NaturalLanguageProcessor _naturalLanguageProcessor;
        private PinHelper _pinHelper = new PinHelper();
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor;

        public AIProcessingService(NaturalLanguageProcessor naturalLanguageProcessor,
            SeleniumMapsUrlProcessor seleniumMapsUrlProcessor)
        {
            _naturalLanguageProcessor = naturalLanguageProcessor;
            _seleniumMapsUrlProcessor = seleniumMapsUrlProcessor;
        }

        public void AIProcessing(List<Topic> topics)
        {
            int aiQueries = 0;
            int urlsCreated = 0;
            if (topics == null) return;

            for (int i = 0; i < topics.Count; i++)
            {
                var topic = topics[i];
                if (!topic.HasMapPin() && topic.AiParsed == false)
                {
                    aiQueries++;
                    var restaurantName = "";
                    var newUrl = ProcessTopic(topic, ref restaurantName);
                    if (newUrl != null)
                    {
                        UpdatePinList(newUrl, topic, ref urlsCreated);
                    }
                }
            }
            Console.WriteLine($"AI Queries : {aiQueries}");
            Console.WriteLine($"AI Urls created : {urlsCreated}");
        }

        public string? ProcessTopic(Topic topic, ref string restaurantName)
        {
            if (!topic.HasMapPin() && topic.AiParsed == false)
            {
                var aiResponse = _naturalLanguageProcessor.Process(topic.Title);
                restaurantName = "";
                var city = "";
                topic.AiParsed = true;
                foreach (var item in aiResponse)
                {
                    if (item.Category == EntityCategory.Location && item.SubCategory == null)
                    {
                        restaurantName = item.Text;
                    }
                    if (item.Category == EntityCategory.Location && item.SubCategory == "City")
                    {
                        city = item.Text;
                    }

                    if (topic.AiTitleInfoV2 == null) topic.AiTitleInfoV2 = new List<AiInformation>();
                    topic.AiTitleInfoV2.Add(new AiInformation()
                    {
                        Text = item.Text,
                        Category = item.Category.ToString(),
                        SubCategory = item.SubCategory,
                        ConfidenceScore = item.ConfidenceScore,
                        Length = item.Length,
                        Offset = item.Offset
                    });
                }

                // Skip if all we have is the city name
                if (!string.IsNullOrEmpty(restaurantName)
                    && restaurantName != "home"
                    && restaurantName != "house"
                    && restaurantName != "shops"
                    && restaurantName != "eating places"
                    && restaurantName != "food booth halls"
                    && restaurantName != "supermarkets"
                    && restaurantName != "airlines"
                    && restaurantName != "hotels"
                    && restaurantName != "Disneys"
                    && restaurantName != "Tokyo Station"
                    && restaurantName != "restaurants"
                    && restaurantName != "Okayama Prefecture"
                    && restaurantName != "tokyodomemarche"
                    && restaurantName != "grocery stores"
                    && restaurantName != "My Life supermarket"
                    && restaurantName != "7-Eleven"
                    && restaurantName != "countries"
                    && restaurantName != "ryokan"
                    && restaurantName != "place"
                    && restaurantName != "restaurante"
                    && restaurantName != "river"
                    && restaurantName != "shop"
                    && restaurantName != "bakeries"
                    && restaurantName != "hotel"
                    && restaurantName != "supermercado"
                    && restaurantName != "supermarket"
                    && restaurantName != "cafes"
                    && restaurantName != "hotel room"
                    && restaurantName != "entrance"
                    && restaurantName != "parks"
                    && restaurantName != "store"
                    && restaurantName != "building"
                    && !restaurantName.StartsWith("#"))
                {
                    var mapsLink = $"http://maps.google.com/?q={restaurantName},{city}";
                    // TODO: Hacks
                    var newUrl = _seleniumMapsUrlProcessor.CheckUrlForMapLinks(mapsLink);
                    return newUrl;
                }
            }

            return null;

        }

        public void UpdatePinList(string newUrl, Topic topic, ref int urlsCreated)
        {
            newUrl = _seleniumMapsUrlProcessor.GetCurrentUrl();
            newUrl = HttpUtility.UrlDecode(newUrl);
            var pin = _pinHelper.TryToGenerateMapPin(newUrl);
            if (pin != null)
            {
                urlsCreated++;
                if (topic.UrlsV2 == null) topic.UrlsV2 = new List<TopicLink>();
                topic.UrlsV2.Add(new TopicLink()
                {
                    AiGenerated = true,
                    Pin = pin,
                    Url = newUrl

                });
            }

        }


    }
}
