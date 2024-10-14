using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class StringHelper
    {
        public static List<string> ExtractHashtags(string text)
        {
            // Define a regular expression to match hashtags
            string pattern = @"#\w+";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(text);

            // Store the extracted hashtags in a list
            List<string> hashtags = new List<string>();
            foreach (Match match in matches)
            {
                hashtags.Add(match.Value);
            }

            return hashtags;
        }

        public static List<string> ExtractUrls(string text)
        {
            // Define a regular expression to match URLs
            string pattern = @"https?://[^\s]+";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(text);

            // Store the extracted URLs in a list
            List<string> urls = new List<string>();
            foreach (Match match in matches)
            {
                urls.Add(match.Value);
            }

            return urls;
        }
    }
}
