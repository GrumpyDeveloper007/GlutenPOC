using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Helper
{
    /// <summary>
    /// Helper functions for currency symbol detection 
    /// </summary>
    public static class CurrencyHelper
    {
        static readonly IEnumerable<string?> currencySymbols;

        static CurrencyHelper()
        {
            // Get all cultures
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            // Extract currency symbols from each culture
            currencySymbols = cultures
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

        }

        /// <summary>
        /// Tries to work out of the text contains a currency symbol
        /// </summary>
        public static bool ContainsCurrencySymbol(string text)
        {
            if (text.Contains('¥')
                || text.Contains('￥')
                || text.Contains("₫1")
                || text.Contains("TWD")
                || text.Contains("SGD")
                || text.Contains("USD")
                || text.Contains("MXN")
                || text.Contains("HKD")
                || text.Contains("lei")
                || text.Contains("RSD")
                || text.Contains("PEN")
                || text.Contains("PAB")
                || text.Contains("NZD")
                || text.Contains("MAD")
                || text.Contains("COP")
                || text.Contains("CLP")
                || text.Contains("CHP")
                || text.Contains("CHF")
                || text.Contains("CAD")
                || text.Contains("ARS")
                || text.Contains("AED")
                || text.Contains("ALL")
                || text.Contains("SAR")
                || text.Contains("DOP")
                || text.Contains("RM")
                || text.Contains("R$")
                || text.Contains("Rp")
                || text.Contains("Kr")
                || text.Contains("kr")//kr & nbsp; 100–300
                || text.Contains("Kč")
                || text.Contains("Rs")
                || text.Contains("Ft")
                || text.Contains("R&")
                || text.Contains("Q&")
                || text.Contains("K&")
                || text.Contains("QAR")
                || text.Contains("UZS")

                || text.Contains('฿')
                || text.Contains('₱')
                || text.Contains('₩')
                || text.Contains('₫')
                || text.Contains('€')
                || text.Contains('£')
                || text.Contains('៛')
                || text.Contains('₹')
                || text.Contains('$')
                || text.Contains('₪')
                || text.Contains('₺')
                || text.Contains('₾')
                || text.Contains('₡')
                || text.Contains("zł")
)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to work out if the text starts with a currency symbol
        /// </summary>
        public static bool StartsWithCurrencySymbol(string text)
        {
            if (text.StartsWith('¥')
                || text.StartsWith('￥')
                || text.StartsWith("₫1")
                || text.StartsWith("TWD")
                || text.StartsWith("SGD")
                || text.StartsWith("USD")
                || text.StartsWith("MXN")
                || text.StartsWith("HKD")
                || text.StartsWith("lei")
                || text.StartsWith("RSD")
                || text.StartsWith("PEN")
                || text.StartsWith("PAB")
                || text.StartsWith("NZD")
                || text.StartsWith("MAD")
                || text.StartsWith("COP")
                || text.StartsWith("CLP")
                || text.StartsWith("CHP")
                || text.StartsWith("CHF")
                || text.StartsWith("CAD")
                || text.StartsWith("ARS")
                || text.StartsWith("AED")
                || text.StartsWith("ALL")
                || text.StartsWith("SAR")
                || text.StartsWith("DOP")
                || text.StartsWith("UZS")
                || text.StartsWith("RM")
                || text.StartsWith("R$")
                || text.StartsWith("Rp")
                || text.StartsWith("Kr")
                || text.StartsWith("kr")//kr & nbsp; 100–300
                || text.StartsWith("Kč")
                || text.StartsWith("R&") // TODO: Not exactly but it fits
                || text.StartsWith("Q&")
                || text.StartsWith("K&")
                || text.StartsWith("QAR")
                || text.StartsWith("Rs")
                || text.StartsWith("Ft")
                || text.StartsWith('฿')
                || text.StartsWith('₱')
                || text.StartsWith('₩')
                || text.StartsWith('₫')
                || text.StartsWith('€')
                || text.StartsWith('£')
                || text.StartsWith('៛')
                || text.StartsWith('₹')
                || text.StartsWith('₪')
                || text.StartsWith('₺')
                || text.StartsWith('₾')
                || text.StartsWith('$')
                || text.StartsWith('₡')
                || text.StartsWith("zł")
)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to work out if the text ends with a currency symbol
        /// </summary>
        public static bool EndsWithCurrencySymbol(string text)
        {
            if (text.EndsWith('¥')
                || text.EndsWith('￥')
                || text.EndsWith("₫1")
                || text.EndsWith("TWD")
                || text.EndsWith("SGD")
                || text.EndsWith("USD")
                || text.EndsWith("MXN")
                || text.EndsWith("HKD")
                || text.EndsWith("lei")
                || text.EndsWith("RSD")
                || text.EndsWith("PEN")
                || text.EndsWith("PAB")
                || text.EndsWith("NZD")
                || text.EndsWith("MAD")
                || text.EndsWith("COP")
                || text.EndsWith("CLP")
                || text.EndsWith("CHP")
                || text.EndsWith("CHF")
                || text.EndsWith("CAD")
                || text.EndsWith("ARS")
                || text.EndsWith("AED")
                || text.EndsWith("ALL")
                || text.EndsWith("SAR")
                || text.EndsWith("DOP")
                || text.EndsWith("UZS")
                || text.EndsWith("RM")
                || text.EndsWith("R$")
                || text.EndsWith("Rp")
                || text.EndsWith("Kr")
                || text.EndsWith("kr")//kr & nbsp; 100–300
                || text.EndsWith("Kč")
                || text.EndsWith("R&") // TODO: Not exactly but it fits
                || text.EndsWith("Q&")
                || text.EndsWith("K&")
                || text.EndsWith("QAR")
                || text.EndsWith("Rs")
                || text.EndsWith("Ft")
                || text.EndsWith('฿')
                || text.EndsWith('₱')
                || text.EndsWith('₩')
                || text.EndsWith('₫')
                || text.EndsWith('€')
                || text.EndsWith('£')
                || text.EndsWith('៛')
                || text.EndsWith('₹')
                || text.EndsWith('₪')
                || text.EndsWith('₺')
                || text.EndsWith('₾')
                || text.EndsWith('$')
                || text.EndsWith('₡')
                || text.EndsWith("zł")
)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Contains currency symbol?
        /// </summary>
        public static bool IsCurrencySymbol(string text)
        {
            if (ContainsCurrencySymbol(text))

            {
                return true;
            }


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
