using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gluten.Core.Helper
{
    /// <summary>
    /// General string helper functions
    /// </summary>
    public class StringHelper
    {
        public static string Truncate(string? value, int maxLength, string truncationSuffix = "…")
        {
            return (value?.Length > maxLength
                ? string.Concat(value.AsSpan(0, maxLength), truncationSuffix)
                : value) ?? "";
        }

        /// <summary>
        /// Makes names more comparable by removing irrelevant characters 
        /// </summary>
        public static string RemoveIrrelevantChars(string text)
        {
            return text.Replace(" ", "").Replace("-", "").Replace("’", "").Replace("'", "").Replace("+", "");
        }

        public static string ReplaceIrrelevantChars(string text)
        {
            return text.Replace("-", " ").Replace("’", " ").Replace("'", " ").Replace("+", " ").Replace(".", "");
        }

        public static string RemoveUnicode(string text)
        {
            string pattern = @"[^\u0000-\u007F]";
            return Regex.Replace(text, pattern, "");
        }


        /// <summary>
        /// Tries to improve comparison results by replacing accent characters
        /// </summary>
        public static string RemoveDiacritics(string text)
        {
            if (text == null) return "";
            var normalizedString = text.Replace("é", "e").Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}
