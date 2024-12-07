using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Helper
{
    public static class CurrencyHelper
    {
        public static bool StartsWithCurrencySymbol(string text)
        {
            if (text.Contains('¥')
                || text.Contains('￥')
                || text.Contains("₫1")
                || text.Contains("TWD")
                || text.Contains("SGD")
                || text.Contains("USD")
                || text.Contains("MXN")
                || text.Contains("HKD")
                || text.Contains("RM")
                || text.Contains("Rp")
                || text.Contains("Rs")
                || text.Contains('฿')
                || text.Contains('₱')
                || text.Contains('₩')
                || text.Contains('₫')
                || text.Contains('€')
                || text.Contains('£')
                || text.Contains('៛')
                || text.Contains('₹')
                || text.Contains('$'))
            {
                return true;
            }
            return false;
        }
        public static bool IsCurrencySymbol(string text)
        {
            if (text.Contains('¥')
                || text.Contains('￥')
                || text.Contains("₫1")
                || text.Contains("TWD")
                || text.Contains("SGD")
                || text.Contains("USD")
                || text.Contains("MXN")
                || text.Contains("HKD")
                || text.Contains("RM")
                || text.Contains("Rp")
                || text.Contains("Rs")
                || text.Contains('฿')
                || text.Contains('₱')
                || text.Contains('₩')
                || text.Contains('₫')
                || text.Contains('€')
                || text.Contains('£')
                || text.Contains('៛')
                || text.Contains('₹')
                || text.Contains('$'))
            {
                return true;
            }

            // Get all cultures
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            // Extract currency symbols from each culture
            var currencySymbols = cultures
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name).CurrencySymbol;
                    }
                    catch
                    {
                        return null; // Skip invalid cultures
                    }
                })
                .Where(symbol => !string.IsNullOrEmpty(symbol))
                .Distinct(); // Remove duplicates

            // Check if the input contains any of the currency symbols
            foreach (var symbol in currencySymbols)
            {
                if (symbol != null && text.Contains(symbol))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
