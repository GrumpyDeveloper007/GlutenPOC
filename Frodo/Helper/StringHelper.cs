using Gluten.Data.TopicModel;
using System.Text.RegularExpressions;

namespace Frodo.Helper
{
    /// <summary>
    /// General helper function for string
    /// </summary>
    internal class StringHelper
    {
        public static string Truncate(string? value, int maxLength, string truncationSuffix = "…")
        {
            return (value?.Length > maxLength
                ? string.Concat(value.AsSpan(0, maxLength), truncationSuffix)
                : value) ?? "";
        }

        public static List<string> ExtractHashTags(string text)
        {
            // Define a regular expression to match hashtags
            string pattern = @"#\w+";
            Regex regex = new(pattern);
            MatchCollection matches = regex.Matches(text);

            // Store the extracted hashtags in a list
            List<string> hashtags = [];
            foreach (Match match in matches)
            {
                hashtags.Add(match.Value);
            }

            return hashtags;
        }

        public static List<TopicLink> ExtractUrls(string text)
        {
            // Define a regular expression to match URLs
            //string pattern = @"https?://[^\s]+";
            //string pattern = @"(https?://[^\s\)]+|\b\w+\.\w{2,}(?:/\S*)?)";
            //string pattern = @"https?://[a-zA-Z0-9\-.]+\.[a-zA-Z]{2,}(/[^\s]*)?|www\.[a-zA-Z0-9\-.]+\.[a-zA-Z]{2,}(/[^\s]*)?|\b[a-zA-Z0-9\-.]+\.[a-zA-Z]{2,}(/[^\s]*)?";
            //string pattern = @"https?://[a-zA-Z0-9\-._~:/?#\[\]@!$&'()*+,;=%]+";
            string pattern = @"https?://[^\s\)\]]+|\bwww\.[^\s\)\]]+|\b[a-zA-Z0-9\-.]+\.[a-zA-Z]{2,}(\/[^\s\)\]]*)?";
            //From search
            //string pattern = @"/(?:(?:https?|ftp|file):\/\/|www\.|ftp\.)(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[-A-Z0-9+&@#\/%=~_|$?!:,.])*(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[A-Z0-9+&@#\/%=~_|$])/igm";

            Regex regex = new(pattern);
            MatchCollection matches = regex.Matches(text);

            // Store the extracted URLs in a list
            var urls = new List<TopicLink>();
            foreach (Match match in matches)
            {
                var url = match.Value;
                if (url.EndsWith('.'))
                    url = url.Substring(0, url.Length - 1);
                var topicUrl = new TopicLink() { Url = url };

                var found = false;
                foreach (var item in urls)
                {
                    if (item.Url == url)
                    {
                        found = true;
                    }
                }

                if (!found)
                    urls.Add(topicUrl);
            }

            return urls;
        }
    }
}
