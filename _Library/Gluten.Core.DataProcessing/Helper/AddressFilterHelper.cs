using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Helper
{
    /// <summary>
    /// Provides some extra filtering for the junk AI can select
    /// </summary>
    public static class AddressFilterHelper
    {
        private static readonly List<string> _addressFilters = [
            "( exact location not specified)",
            "(no address ",
            "(no street ",
            "( in the text, but a  is given: ",
            "(No specific ",
            "(shop url not available, only address ",
            "(no street ",
            "<no address provided ",
            "(no street address",
            "(No address provided",
            "(Frozen food section ",
            "(factory location ",
            "(unspecified",
            "(multiple locations",
            "(not ",
            "()",
            "(null)",
            "(Centre location)",
            "(street ",
            "(address ",
            "( in the given",
            "( in given",
            "(closed",
            "(unknown,",
            "(location within",
            "(website link:",
            "<No specific address ",
            "<No street ",
            "<No address",
            "<unspecified",
            "<address not provided ",
            "<Not provided ",
            "<insert ",
            "<unknown>",
            "<not provided>",
            "<address>",
            "<address not ",
            "no specific address provided",
            "no specific location provided in the given text.",
            "Not specified",
            "Google Maps link",
            "I do not have a specific",
            "CBD location",
            "http://",
            "https:",
            "Google Maps address:",
            "not provided",
            "Address not provided in given text",
            "no address found in the snippet, only a google search result link",
            "unknown (located",
            "Available on Uber eats",
            "not explicitly stated in the text,",
            "no specific ",
            "no address ",
            "no explicit ",
            "No street ",
            "unspecified",
            "Various locations ",
            "unknown",
            "Easy to search on Google",
            "n/a (food truck)",
            "I apologize,",
            "www.",
            "facebook.com",
            "N/A",
            "in the given text",
            "123 Main St",
                        "( exact location not specified)",
            "(No specific address mentioned)",
            "(no address provided)",
            "(no specific address given)",
            "(no street address provided in the text)",
            "<No specific address provided in the given text>",
            "<no address provided in the original text>",
            "<address not provided in original text>",
            "<Not provided in original text>",
            "<insert address here>",
            "<insert address>",
            "<unknown>",
            "<not provided>",
            "no specific address provided",
            "Not specified",
            "Google Maps link",
            "Lovely restaurant",
            "by the beach",
            "up the hill",
            "around the corner",
            "off the ",
            "across from ",
            "just up by",
            "Top floor of",
            "you go in the "

];

        /// <summary>
        /// Provides some extra filtering for the junk AI can select
        /// </summary>
        public static string? FilterAddress(string? address)
        {
            if (address == null) return null;
            if (address.Contains("train stops")) return null;
            if (address.Contains("train station")) return null;

            foreach (var filter in _addressFilters)
            {
                if (address.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "";
                }
                //address = address.ToLower().Replace(filter.ToLower(), "");
            }
            return address.Trim();
        }
    }
}
