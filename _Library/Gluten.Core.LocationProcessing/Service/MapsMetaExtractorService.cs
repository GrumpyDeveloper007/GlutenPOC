// Ignore Spelling: Href

using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Helper;
using Gluten.Core.LocationProcessing.Model;
using Gluten.Data.PinCache;
using System.Text.RegularExpressions;

namespace Gluten.Core.LocationProcessing.Service
{
    /// <summary>
    /// Extracts information from maps html
    /// </summary>
    public class MapsMetaExtractorService(IConsole Console)
    {
        public int maxMatchIndex = 0;
        public int minSpanCount = 99;


        /// <summary>
        /// Tries to process the html extracting key information
        /// </summary>
        public PinCacheMeta? ExtractMeta(string? html)
        {
            var root = new LabelNode();
            PinCacheMeta? result = null;
            if (string.IsNullOrWhiteSpace(html)) return null;

            result = new PinCacheMeta
            {
                Price = "",
                RestaurantType = "",
                Stars = ""
            };

            if (html.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
            {
                result.PermanentlyClosed = true;
            }

            HtmlHelper.TraverseHtml(html, root);
            bool isHotel = false;

            foreach (var item in root.Child)
            {
                if (item.Name.Contains("Check in / Check out"))
                {
                    isHotel = true;
                }
            }
            if (root.Buttons.Count > 1 && (
                root.Buttons[1].StartsWith("See rooms")
                || root.Buttons[1].StartsWith("See more rooms")
                || isHotel))
            {
                result = new PinCacheMeta
                {
                    Price = "",
                    RestaurantType = "Hotel",
                    Stars = root.Child[2].Name
                };
            }

            if (root.Child.Count > 1) result.Stars = root.Child[1].Name;
            if (!result.Stars.Contains("stars") && root.Spans.Count > 2
                && StartsWithNumber(root.Spans[2]))
            {
                result.Stars = root.Spans[2].Replace(",", ".") + " stars";
            }
            if (result.Stars.Contains("Actions"))
            {
                result.Stars = "";
            }

            if (result.RestaurantType != "Hotel" && root.Spans.Count > 0)
            {
                if (root.Spans.Count < minSpanCount)
                    minSpanCount = root.Spans.Count;

                var hasPriceRange = root.Spans.Exists(o => CurrencyHelper.StartsWithCurrencySymbol(o) || CurrencyHelper.EndsWithCurrencySymbol(o));
                var hasReviews = root.Spans.Exists(o => o.StartsWith('(') || o.EndsWith(')'));

                int startingIndex = 0;
                if (!hasReviews && !hasPriceRange)
                {
                    startingIndex = 2;
                }

                string previousInfo = "(";
                bool foundPrevious = !hasReviews;
                for (int t = startingIndex; t < root.Spans.Count; t++)
                {
                    var item = root.Spans[t];
                    if (item.StartsWith('')) continue;
                    if (item.StartsWith('')) continue;
                    if (item.StartsWith('')) continue;
                    if (item.StartsWith('')) continue;
                    if (item.StartsWith('·')) continue;

                    if (item.StartsWith(previousInfo)
                        && StartsWithNumber(item.Substring(1, 1)))
                    {
                        foundPrevious = true;
                        continue;
                    }
                    if (!foundPrevious) continue;
                    if (hasPriceRange && CurrencyHelper.ContainsCurrencySymbol(item))
                    {
                        result.Price = item.Replace("&nbsp;", " ");
                        continue;
                    }
                    if (item.Contains("Photos")) break;
                    if (item.Contains("Questions") || item.Contains("Contact"))
                    {
                        if (startingIndex > 0 && !root.Spans[t - 1].Contains("Questions"))
                            result.RestaurantType = root.Spans[t - 1];
                        break;
                    }
                    if (item.Contains("month")) break;
                    if (item.Contains("Opens")) break;
                    if (item.Contains("reviews")) break;
                    if (item.Contains("1 review")) break;
                    if (item.Contains("Permanently closed")) break;


                    result.RestaurantType = item;
                    break;
                }

            }

            result.Price = result.Price.Replace(".", "").Replace("·", "").Replace("", "");

            if (!string.IsNullOrWhiteSpace(result.Price) &&
                !CurrencyHelper.IsCurrencySymbol(result.Price))
            {
                if (result.RestaurantType != result.Price
                    && !string.IsNullOrWhiteSpace(result.Price))
                {
                    Console.WriteLine($"IsCurrencySymbol problem");
                }
                result.Price = "";
            }

            return result;
        }

        private static bool StartsWithNumber(string item)
        {
            if (item.StartsWith('0') || item.StartsWith('1') || item.StartsWith('2')
    || item.StartsWith('3') || item.StartsWith('4') || item.StartsWith('5')
    || item.StartsWith('6') || item.StartsWith('7') || item.StartsWith('8')
    || item.StartsWith('9'))
                return true;
            return false;
        }

        private bool StartsWithWeekdayShort(string item)
        {
            if (item.StartsWith("Mon") || item.StartsWith("Tue") || item.StartsWith("Wed")
    || item.StartsWith("Thu") || item.StartsWith("Fri") || item.StartsWith("Sat")
    || item.StartsWith("Sun6"))
                return true;
            return false;
        }


        private static string RemoveSpacers(string text)
        {
            return text.Replace(".", "").Replace("·", "").Replace("", "").Replace("", "");
        }
    }
}
