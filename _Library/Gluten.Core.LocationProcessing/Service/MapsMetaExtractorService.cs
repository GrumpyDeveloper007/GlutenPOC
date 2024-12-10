// Ignore Spelling: Href

using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Helper;
using Gluten.Core.LocationProcessing.Model;
using Gluten.Data.PinCache;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Extracts information from maps html
    /// </summary>
    public class MapsMetaExtractorService(RestaurantTypeService _restaurantTypeService, IConsole Console)
    {
        /// <summary>
        /// Tries to process the html extracting key information
        /// </summary>
        public PinCacheMeta? ExtractMeta(string? html)
        {
            var root = new LabelNode();
            PinCacheMeta? result = null;
            if (!string.IsNullOrWhiteSpace(html))
            {
                HtmlHelper.TraverseHtml(html, root);
                bool isHotel = false;

                foreach (var item in root.Child)
                {
                    if (item.Name.Contains("Check in / Check out"))
                    {
                        isHotel = true;
                    }
                }

                if (root.Buttons.Count == 1)
                {
                    result = new PinCacheMeta
                    {
                        RestaurantType = root.Buttons[0],
                    };
                }
                else if (root.Buttons.Count == 0)
                {
                    // ignore - normally region/city name
                    result = new PinCacheMeta
                    {
                        Price = "",
                        RestaurantType = "",
                        Stars = ""
                    };
                }
                else if (root.Buttons[1].StartsWith("See rooms")
                    || root.Buttons[1].StartsWith("See more rooms")
                    || isHotel)
                {
                    result = new PinCacheMeta
                    {
                        Price = "",
                        RestaurantType = "Hotel",
                        Stars = root.Child[2].Name
                    };

                }
                else
                {
                    result = new PinCacheMeta
                    {
                        Price = root.Child[3].Name,
                        RestaurantType = root.Buttons[1],
                        Stars = root.Child[1].Name
                    };
                    if (!result.Price.StartsWith("Price:"))
                    {
                        result.Price = "";
                    }
                    if (result.Price == "")
                    {
                        if (root.Spans.Count >= 7)
                        {
                            result.Price = root.Spans[6];
                            if (result.Price.StartsWith('('))
                            {
                                result.Price = root.Spans[7];
                            }
                        }

                        if (!CurrencyHelper.IsCurrencySymbol(result.Price))
                        {
                            if (result.RestaurantType != result.Price && result.Price != "Permanently closed"
                                && !string.IsNullOrWhiteSpace(result.Price))
                            {
                                Console.WriteLine($"IsCurrencySymbol problem");
                            }
                            result.Price = "";
                        }
                    }
                    if (result.RestaurantType == "Photos" && root.Spans.Count > 5)
                    {
                        result.RestaurantType = root.Spans[5];
                    }

                    if (result.RestaurantType.Contains("review", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Unknown restaurant type : {result.RestaurantType}");
                        result.RestaurantType = "";
                    }
                    if (result.RestaurantType.Contains("review", StringComparison.InvariantCultureIgnoreCase)
                        || result.RestaurantType == "."
                        || result.RestaurantType == "")
                    {
                        result.RestaurantType = "";
                    }

                    if (!result.Stars.Contains("stars"))
                    {
                        result.Stars = root.Spans[2].Replace(",", ".") + " stars";
                    }
                }
                if (html.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.PermanentlyClosed = true;
                }
                _restaurantTypeService.AddRestaurantType(result.RestaurantType);
            }

            return result;
        }
    }
}
