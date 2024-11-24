using Gluten.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Frodo.Service
{
    internal class MapsMetaExtractorService
    {
        private List<string> _restaurantTypes = [];

        private List<string> _ignoreList = [
            "Train station",
            "Airports",
            "airport",
            "Sightseeing tour agency",
            "Laundromat",
            "Historical landmark",
            "Island",
            "Electronics manufacturer",
            "Corporate office",
            "Theme park",
            "Subway station",
            "Food manufacturer",
            "Art museum",
            "International airport",
            "Airport",
            "Garden",
            "Cinema",
            "Manufacturer",
            "Observation deck",
            "Mountain peak",
            "Amusement park",
            "Bridge",
            "Soy sauce maker",
            "Housing development",
            "Massage spa",
            "Waterfall",
            "Delivery service",
            "Water treatment supplier",
            "River",
            "Event venue",
            "Museum",
            "Florist",
            "Park",
            "Tourist attraction",
            "Business park",
            "Hair salon",
            "Holiday apartment",
            "Car racing track",
            "Language school",
            "Host club",
            "Shinto shrine",
            "Cultural center",
            "Foreign consulate",
            "Non-profit organization",
            "Truck parts supplier",
            "Gift shop",
            "Lake",
            "Spa",
            "Festival",
            "Beach",
            "Dog trainer",
            "Concert hall",
            "Tour operator",
            "Art gallery",
            "Health and beauty sho",
            "public bath",
            "review",
            "City park",
            "Community center",
            "Cooking class",
            "Coworking space",
            "Garment exporter",
            "General hospital",
            "Medical clinic",
            "Modern art museum",
            "Mobile caterer",
            "Scenic spot",
            "public bath",
            "Yoga studio"
            ];

        class LabelNode
        {
            public string Name { get; set; } = "";
            public List<LabelNode> Child { get; set; } = [];

            public List<string> Buttons { get; set; } = [];
        }

        public List<string> GetRestuarantTypes()
        {
            _restaurantTypes.Sort();
            var data = new List<string>();
            foreach (var item in _restaurantTypes)
            {
                if (!item.Contains("shop", StringComparison.InvariantCultureIgnoreCase)
                    && !item.Contains("store", StringComparison.InvariantCultureIgnoreCase)
                    && !item.Contains("market", StringComparison.InvariantCultureIgnoreCase)
                    && !item.Contains("hotel", StringComparison.InvariantCultureIgnoreCase)
                    && !_ignoreList.Any(s => item.Contains(s, StringComparison.InvariantCultureIgnoreCase))
                    )
                {
                    data.Add(item);
                }
            }
            return data;
        }

        public PinCacheMeta? ExtractMeta(string? html)
        {
            var root = new LabelNode();
            PinCacheMeta? result = null;
            if (!string.IsNullOrWhiteSpace(html))
            {
                TraverseHtml(html, root);
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
                    if (result.RestaurantType.Contains("review", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Unknown restaurant type : {result.RestaurantType}");
                    }
                    if (result.RestaurantType.Contains("review", StringComparison.InvariantCultureIgnoreCase)
                        || result.RestaurantType == "."
                        || result.RestaurantType == "")
                    {
                        result.RestaurantType = "";
                    }

                    if (!result.Stars.Contains("stars"))
                    {
                        result.Stars = "";
                    }
                }
                if (html.Contains("permanently closed", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.PermanentlyClosed = true;
                }
                if (!string.IsNullOrWhiteSpace(result.RestaurantType) && !_restaurantTypes.Contains(result.RestaurantType))
                {
                    _restaurantTypes.Add(result.RestaurantType);
                }

            }

            return result;
        }

        public void AddRestaurantType(string restaurant)
        {
            if (!string.IsNullOrWhiteSpace(restaurant) && !_restaurantTypes.Contains(restaurant))
            {
                _restaurantTypes.Add(restaurant);
            }
        }

        public void ClearRestaurantType()
        {
            _restaurantTypes.Clear();
        }


        private static void TraverseHtml(string html, LabelNode rootNode)
        {
            // Load the HTML into an HtmlDocument
            HtmlDocument document = new();
            document.LoadHtml(html);

            // Get the root node
            HtmlNode root = document.DocumentNode;

            // Traverse all nodes
            TraverseNode(root, 0, rootNode);
        }

        private static void TraverseNode(HtmlNode node, int depth, LabelNode parent)
        {
            var currentParent = parent;

            // Print attributes if the node has any
            if (node.Attributes.Count > 0)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.Name == "aria-label")
                    {
                        currentParent = new LabelNode
                        {
                            Name = attribute.Value,
                        };
                        parent.Child.Add(currentParent);
                    }
                }
            }

            if (node.Name == "button" && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                currentParent.Buttons.Add(node.InnerText);
            }

            // Recurse through child nodes
            foreach (var childNode in node.ChildNodes)
            {
                TraverseNode(childNode, depth + 1, currentParent);
            }
        }

    }
}
