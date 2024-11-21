using Gluten.Data.TopicModel;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class MapsMetaExtractorService
    {
        public List<string> _restaurantTypes = new();

        class LabelNode
        {
            public string Name { get; set; }
            public List<LabelNode> Child { get; set; } = new();

            public List<string> Buttons { get; set; } = new();
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
                        result = result;
                    }
                    if (result.RestaurantType.Contains("reviews", StringComparison.InvariantCultureIgnoreCase)
                        || result.RestaurantType == "."
                        || result.RestaurantType == "")
                    {
                        result.RestaurantType = "";
                    }

                    // TODO: Extract a list and add support for filtering in client app
                    if (!string.IsNullOrWhiteSpace(result.RestaurantType)
                        && !result.RestaurantType.Contains("restaurant", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("snack", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("cafe", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("cake", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Confectionery", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Convenience", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Pub", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("market", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("shop", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("store", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Creperie", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Yakatabune", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Bakery", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("和食店", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Crab House", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Bar", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Hawker stall", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("steakhouse", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("steak house", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Food court", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("ベーカリー", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Patisserie", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("ケーキ屋", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("delicatessen", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("takeaway", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("居酒屋", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("teahouse", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("tea house", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("beer", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Bristo", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Bistro", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Brew", StringComparison.InvariantCultureIgnoreCase)
                        && !result.RestaurantType.Contains("Deli", StringComparison.InvariantCultureIgnoreCase)

                        )
                    {
                        result = result;
                    }
                    if (!result.Stars.Contains("stars"))
                    {
                        result.Stars = "";
                    }
                }
                _restaurantTypes.Add(result.RestaurantType);

            }

            return result;
        }


        private static void TraverseHtml(string html, LabelNode rootNode)
        {
            // Load the HTML into an HtmlDocument
            HtmlDocument document = new HtmlDocument();
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
