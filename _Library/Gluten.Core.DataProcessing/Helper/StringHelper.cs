using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Helper
{
    /// <summary>
    /// General string helper functions
    /// </summary>
    internal class StringHelper
    {
        /// <summary>
        /// Makes names more comparable by removing irrelevant characters 
        /// </summary>
        public static string RemoveIrrelevantChars(string text)
        {
            return text.Replace(" ", "").Replace("-", "");
        }

        /// <summary>
        /// Tries to improve comparison results by replacing accent characters
        /// </summary>
        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
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
