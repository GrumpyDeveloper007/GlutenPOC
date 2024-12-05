using Gluten.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Frodo.Helper
{
    internal static class LabelHelper
    {
        static List<string> wordMatch;

        public static void Reset()
        {
            wordMatch = [];
        }

        public static void Check()
        {
            Console.WriteLine(wordMatch);
        }

        static string RemoveTextWithinParentheses(string input)
        {
            // Use a regular expression to match text within parentheses
            string pattern = @"\([^)]*\)"; // Matches '(' followed by any character except ')' repeated, then ')'
            string replacement = ""; // Replace with an empty string
            return Regex.Replace(input, pattern, replacement);
        }

        public static bool IsInTextBlock(string matchText, string titleText)
        {
            var title = StringHelper.RemoveIrrelevantChars(StringHelper.RemoveDiacritics(titleText));

            var label = StringHelper.RemoveDiacritics(HttpUtility.UrlDecode(matchText));
            var words = StringHelper.RemoveUnicode(StringHelper.ReplaceIrrelevantChars(label)).Split(' ');
            var wordcount = label.Split(' ').Count();

            if (wordcount > 2)
            {
                label = $"{words[0]} {words[1]}";
            }
            label = StringHelper.RemoveIrrelevantChars(label);
            label = label.Replace("Canteen", "");
            label = RemoveTextWithinParentheses(label);
            var wordMatchCount = 0;
            var matchWord = "";
            if (!title.Contains(label, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var word in words)
                {
                    if (string.IsNullOrWhiteSpace(word)) continue;
                    var newWord = word;
                    if (newWord[newWord.Length - 1] == 's') newWord = newWord.Substring(0, newWord.Length - 1);
                    if (title.Contains(newWord, StringComparison.InvariantCultureIgnoreCase))
                    {
                        wordMatchCount++;
                        matchWord = newWord;
                    }
                }
                if (wordMatchCount >= 2) return true;
                if (wordMatchCount == 1 && matchWord == words[0]) return true;
                List<string> oneWordMatchList = ["Nino", "7-Eleven", "Ricca", "Hazuki", "Mos",
                    "McDonald","SOPH","Tsukiji","Sushiro","anbosakikonosaki","Alice","emu","Denny","Starbucks"
                    ,"OOTOYA","Aming","ICHiMARU5","Papillon","Lopia","Gonpachi","bills","ICHIRAN","Hanamasa"
                    ,"Coop","KALDI","Aeon","kijitora","SUSHIRO","anbosakikonosaki","YAKITORI","Ninigi"
                    ,"Yoshinoya","AFURI","SATSUKI","Soph","ENYA","Morimoto","Gokuraku","Doutor","sunlight"
                    ,"Hummingbird","Oasis","Lovegan","MAAZI","tostada","Makro","Jollibee","Shopwise","GS25"
                    ,"Emart","Coupang","Izumiya","SHINKO","Nooks","Anantara","Tartine","Baan","Sarnies","Kith"
                    ,"Cedele","Marketplace","VIVO","Movenpick","Skirt","Giant","Bali_Creation_Batik"
                    ,"BATIK","Dolcemare","Kayumas","Frakenseins","Bambusa","LACALITA","Barbarossa+Sanur"
                    ,"Pulau+Sukun","PT.+Jakarta+Kokoku+Intech","Malaika","Oolaa","Pici"];
                matchWord = words[0];
                if (wordMatchCount == 1 &&
                    (oneWordMatchList.Exists(o => matchWord.Contains(o, StringComparison.InvariantCultureIgnoreCase))
                    || oneWordMatchList.Exists(o => o.Contains(matchWord, StringComparison.InvariantCultureIgnoreCase))
                    )) return true;

                foreach (var word in words)
                {
                    if (title.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!wordMatch.Exists(o => word == o))
                        {
                            wordMatch.Add(word);
                        }
                    }
                }

                return false;
            }
            return true;
        }
    }
}
