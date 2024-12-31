using Frodo.Helper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    /// <summary>
    /// Add various meta data to topics
    /// </summary>
    internal class TopicMetaCreationService(
        TopicsDataLoaderService _topicsLoaderService,
        AiInterfaceService _localAi,
        GeoService _geoService,
        CityService _cityService,
        FBGroupService _fBGroupService,
        IConsole Console)
    {
        /// <summary>
        /// Scan topics create short titles that can be shown as web link text
        /// </summary>
        public async Task GenerateShortTitles(List<DetailedTopic> Topics)
        {
            int shortTitlesAdded = 0;
            int itemsUpdated = 0;
            for (int i = 0; i < Topics.Count; i++)
            {
                var topic = Topics[i];
                if (itemsUpdated > 50)
                {
                    Console.WriteLineBlue("Saving topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }

                if (topic.ShortTitleProcessed) continue;
                if (i % 1000 == 0)
                    Console.WriteLine($"Generating short title {i} of {Topics.Count}");

                if ((topic.AiVenues == null || topic.AiVenues.Count == 0)
                    && (topic.UrlsV2 == null || topic.UrlsV2.Count == 0)
                    && (topic.ResponsesV2 == null || topic.ResponsesV2.Count == 0)) continue;

                if (string.IsNullOrWhiteSpace(topic.ShortTitle))//&& !topic.ShortTitleProcessed
                {
                    topic.ShortTitle = await _localAi.GenerateShortTitle(topic.Title);
                    topic.ShortTitleProcessed = true;
                    if (!string.IsNullOrWhiteSpace(topic.ShortTitle))
                    {
                        shortTitlesAdded++;
                        itemsUpdated++;
                    }
                }
            }

            _topicsLoaderService.SaveTopics(Topics);
            Console.WriteLine($"Added short title : {shortTitlesAdded}");
        }

        /// <summary>
        /// Improve the chances of geo locating restaurants by trying to works out the country and city
        /// </summary>
        public async Task ScanTopicsDetectCountryAndCity(List<DetailedTopic> Topics)
        {
            var countries = _geoService.GetCountries();
            var unknownCities = new List<string>();
            var itemsUpdated = 0;

            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];

                if (topic.AiVenues == null || topic.AiVenues.Count == 0) continue;
                if (TopicItemHelper.IsRecipe(topic)) continue;
                if (TopicItemHelper.IsTopicAQuestion(topic)) continue;
                if (!string.IsNullOrWhiteSpace(Topics[i].TitleCountry) && !string.IsNullOrWhiteSpace(Topics[i].TitleCity)) continue;
                //if (topic.TitleCategory != "DESCRIBE") continue;
                if (topic.CitySearchDone) continue;
                if (itemsUpdated > 100)
                {
                    Console.WriteLineBlue("Saving Topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }

                Console.WriteLine($"Extracting city/country names {i} of {Topics.Count}");

                if (string.IsNullOrWhiteSpace(topic.TitleCity))
                {
                    var city = await _localAi.ExtractCity(topic.Title);
                    city = city.Replace("\r", "");
                    city = city.Replace("\n", "").Trim();

                    topic.CitySearchDone = true;
                    if (topic.Title.Contains(city))
                    {
                        if (_cityService.IsCity(city))
                        {
                            topic.TitleCity = city;
                        }
                        else
                        {
                            Console.WriteLine($"Unknown city {city}");
                            if (!unknownCities.Exists(o => o == city))
                                unknownCities.Add(city);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"City not found in message");
                    }
                }

                // todo: remove me
                if (!_fBGroupService.IsGenericGroup(topic.GroupId)) continue;

                if (string.IsNullOrWhiteSpace(topic.TitleCountry))
                {
                    var country = await _localAi.ExtractCountry(topic.Title) ?? "";
                    country = country.Replace("\r", "");
                    country = country.Replace("\n", "").Trim();
                    country = country.Replace("\"", "");

                    country = CapitalizeEachWord(country);

                    country = country.Replace("United States of America", "United States");
                    country = country.Replace("USA", "United States");
                    country = country.Replace("UK", "United Kingdom");
                    country = country.Replace("Wales", "United Kingdom");
                    country = country.Replace("The Netherlands", "Netherlands");
                    country = country.Replace("Nederland", "Netherlands");
                    country = country.Replace("Suid Afrika", "South Africa");
                    country = country.Replace("Türkiye", "Turkey");
                    country = country.Replace("Saint Lucia", "St. Lucia");
                    country = country.Replace("Bahamas", "The Bahamas");
                    country = country.Replace("Turks and Caicos Islands", "Turks & Caicos Is.");
                    country = country.Replace("Grand Cayman", "Cayman Is.");
                    country = country.Replace("Saint Kitts and Nevis", "St.Kitts & Nevis");
                    country = country.Replace("Cayman Islands", "Cayman Is.");
                    country = country.Replace("Giappone", "Japan");
                    country = country.Replace("Japón", "Japan");
                    country = country.Replace("Hong Kong", "China");
                    country = country.Replace("VIETNAM", "Vietnam");
                    country = country.Replace("Viet Nam", "Vietnam");
                    country = country.Replace("ไทย", "Thailand");
                    country = country.Replace("ประเทศThailand", "Thailand");
                    country = country.Replace("Kingdom of Thailand", "Thailand");
                    country = country.Replace("Gambia", "The Gambia");
                    country = country.Replace("Antigua", "Antigua & Barbuda");
                    country = country.Replace("Bosnia and Herzegovina", "Bosnia & Herzegovina");
                    country = country.Replace("North Macedonia", "Macedonia");
                    country = country.Replace("Ελλάδα", "Greece");
                    country = country.Replace("Mallorca", "Spain");
                    country = country.Replace("Kosovo", "Albania");
                    country = country.Replace("Republic of Malta", "Malta");
                    country = country.Replace("Belgium --", "Belgium");
                    country = country.Replace("Canary islands", "Spain");
                    country = country.Replace("Gran Canaria", "Spain");
                    country = country.Replace("Peru", "Peru (Peruvian point of view)");
                    country = country.Replace("Perú", "Peru (Peruvian point of view)");


                    country = country.Replace("Bali", "Indonesia");
                    country = country.Replace("Czechia", "Czech Republic");
                    if (country.StartsWith("Korea"))
                        country = country.Replace("Korea", "South Korea");

                    if (string.IsNullOrWhiteSpace(country)) continue;

                    if (country == "Europe") continue;

                    if (!countries.Exists(o => o == country)
                        && !countries.Exists(o => o == country.Replace("é", "e"))
                        && country != "Asia"
                        && country != "Europe" && country != "European Union" && country != "European"
                        && country != "South America"
                        )
                    {

                        if (country != "Safe") Console.WriteLineRed($"Unknown country : {country}");
                    }
                    else
                    {
                        topic.TitleCountry = country.Replace("é", "e");
                    }
                }

                if (topic.TitleCity != null)
                {
                    var countryForCity = _cityService.CityToCountry(topic.TitleCity);
                    var groupCountry = _fBGroupService.GetCountryName(topic.GroupId);
                    if (string.IsNullOrWhiteSpace(groupCountry)) groupCountry = topic.TitleCountry ?? "";
                    if (!string.IsNullOrWhiteSpace(countryForCity) && !countryForCity.Contains(groupCountry, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLineRed($"Group Country :{groupCountry}, city country :{countryForCity}");
                    }
                }
                itemsUpdated++;

            }

            Console.WriteLineBlue("Unknown cities");
            foreach (var city in unknownCities)
            {
                Console.WriteLineBlue(city);
            }


            _topicsLoaderService.SaveTopics(Topics);
        }

        /// <summary>
        /// Provides translated topic text for non-English topics
        /// </summary>
        public async Task TranslateTopic(List<DetailedTopic> Topics)
        {
            var itemsUpdated = 0;

            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];

                if (itemsUpdated > 100)
                {
                    Console.WriteLineBlue("Saving Topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }

                if (!string.IsNullOrWhiteSpace(topic.TitleLanguage)
                    && topic.TitleLanguage != "English"
                    && topic.TitleLanguage != "Japanese"
                    && string.IsNullOrWhiteSpace(topic.TitleEnglish)
                    && !string.IsNullOrWhiteSpace(topic.Title))
                {
                    var text = await _localAi.TranslateToEnglish(topic.Title);
                    Console.WriteLine($"Translate topics {i} of {Topics.Count} : ");
                    topic.TitleEnglish = text;
                    itemsUpdated++;
                }

            }

            _topicsLoaderService.SaveTopics(Topics);
        }


        /// <summary>
        /// Try to work out the language of the topic
        /// </summary>
        public async Task CalculateLanguageTopic(List<DetailedTopic> Topics)
        {
            var itemsUpdated = 0;

            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];

                if (string.IsNullOrWhiteSpace(topic.Title)) continue;
                if (!string.IsNullOrWhiteSpace(topic.TitleLanguage)) continue;
                if (itemsUpdated > 100)
                {
                    Console.WriteLineBlue("Saving Topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }

                var language1 = await _localAi.CalculateLanguage(topic.Title);
                var language2 = await _localAi.CalculateLanguage(topic.Title);
                var language3 = await _localAi.CalculateLanguage(topic.Title);
                var language = language1;
                if (language2 == language3) language = language2;
                if (language1.Length > 10 || language2.Length > 10 || language3.Length > 10)
                {
                    Console.WriteLine($"Categorise topics {i} of {Topics.Count} : {language}");
                }
                if (language != "English")
                {
                    Console.WriteLine($"Categorise topics {i} of {Topics.Count} : {language}");
                }
                Console.WriteLine($"Categorise topics {i} of {Topics.Count} : {language1},{language2},{language3}");
                topic.TitleLanguage = language;
                itemsUpdated++;
            }

            _topicsLoaderService.SaveTopics(Topics);
        }

        /// <summary>
        /// Try to categorise the topics
        /// </summary>
        public async Task CategoriseTopic(List<DetailedTopic> Topics)
        {
            var itemsUpdated = 0;

            for (int i = 0; i < Topics.Count; i++)
            {
                DetailedTopic? topic = Topics[i];

                if (!string.IsNullOrWhiteSpace(topic.TitleCategory)) continue;
                if (itemsUpdated > 100)
                {
                    Console.WriteLineBlue("Saving Topics");
                    _topicsLoaderService.SaveTopics(Topics);
                    itemsUpdated = 0;
                }


                var category = await _localAi.CategoriseMessage(topic.Title);
                Console.WriteLine($"Categorise topics {i} of {Topics.Count} : {category}");
                if (category == "DESCRIBE" || category == "QUESTION" || category == "UNKNOWN")
                {
                    topic.TitleCategory = category;
                }
                else if (category.Contains("DESCRIBE"))
                {
                    topic.TitleCategory = "DESCRIBE";
                }
                else
                {
                    Console.WriteLineRed($"Failed to categorise :{category}");
                }

                itemsUpdated++;
            }

            _topicsLoaderService.SaveTopics(Topics);
        }

        private static string CapitalizeEachWord(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            // Use TextInfo for culture-aware casing
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(str.ToLower());
        }
    }
}
