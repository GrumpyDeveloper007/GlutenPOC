// Ignore Spelling: Href

using Gluten.Core.LocationProcessing.Helper;
using Gluten.Data.PinCache;
using HtmlAgilityPack;
using System;

namespace Gluten.Core.LocationProcessing.Service
{
    public class MapsMetaExtractorService
    {
        private readonly List<string> _restaurantTypes = [];

        private readonly List<string> _ignoreList = [
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

        public class LabelNode
        {
            public string Name { get; set; } = "";
            public List<LabelNode> Child { get; set; } = [];

            public List<string> Buttons { get; set; } = [];

            public List<string> Spans { get; set; } = [];

            public List<string> AriaHidden { get; set; } = [];

            public bool ResultsNode { get; set; }

            public string InnerHtml { get; set; } = "";
            public string Href { get; set; } = "";
        }

        public List<string> GetRestaurantTypes()
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
                else if (root.Buttons.Count == 0)
                {
                    // ignore - normally region/city name
                    result = new PinCacheMeta
                    {
                        Price = "",
                        RestaurantType = "",
                        Stars = ""
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
                    if (result.Price == "")
                    {
                        if (root.Spans.Count >= 7)
                        {
                            result.Price = root.Spans[6];
                            if (result.Price.StartsWith('('))
                            {
                                result.Price = root.Spans[7];
                            }
                        }

                        if (!CurrencyHelper.IsCurrencySymbol(result.Price))
                        {
                            if (result.RestaurantType != result.Price && result.Price != "Permanently closed"
                                && !string.IsNullOrWhiteSpace(result.Price))
                            {
                                Console.WriteLine($"IsCurrencySymbol problem");
                            }
                            result.Price = "";
                        }
                    }
                    if (result.RestaurantType == "Photos" && root.Spans.Count > 5)
                    {
                        result.RestaurantType = root.Spans[5];
                    }

                    if (result.RestaurantType.Contains("review", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Unknown restaurant type : {result.RestaurantType}");
                        result.RestaurantType = "";
                    }
                    if (result.RestaurantType.Contains("review", StringComparison.InvariantCultureIgnoreCase)
                        || result.RestaurantType == "."
                        || result.RestaurantType == "")
                    {
                        result.RestaurantType = "";
                    }

                    if (!result.Stars.Contains("stars"))
                    {
                        result.Stars = root.Spans[2].Replace(",", ".") + " stars";
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

        public void AddRestaurantType(string? restaurant)
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

        public string GetRestaurantType(string html)
        {
            HtmlDocument document = new();
            document.LoadHtml(html);

            var spanNodes = document.DocumentNode.SelectNodes("//span[last()]");

            if (spanNodes.Count > 5)
            {
                return spanNodes[5].InnerText;
            }
            return "";
        }

        public string GetComment(string html)
        {
            HtmlDocument document = new();
            document.LoadHtml(html);

            var spanNodes = document.DocumentNode.SelectNodes("//span[last()]");

            var comment = spanNodes.Last().ParentNode.InnerText;
            if (!comment.Contains("gluten", StringComparison.InvariantCultureIgnoreCase))
            {
                return "";
            }
            return comment;

            // example - 
            //<div class="Nv2PK THOPZb CpccDe " jsaction="mouseover:pane.wfvdle11;mouseout:pane.wfvdle11">
            //	<a class="hfpxzc" aria-label="Caffè Ponte" href="https://www.google.com/maps/place/Caff%C3%A8+Ponte/data=!4m7!3m6!1s0x355aa20d17743d03:0xb07a1b2acf4005d5!8m2!3d34.3939983!4d132.4542247!16s%2Fg%2F1hc0_5tqv!19sChIJAz10Fw2iWjUR1QVAzyoberA?authuser=0&amp;hl=en&amp;rclk=1" jsaction="pane.wfvdle11;focus:pane.wfvdle11;blur:pane.wfvdle11;auxclick:pane.wfvdle11;keydown:pane.wfvdle11;clickmod:pane.wfvdle11" jslog="12690;track:click,contextmenu;mutable:true;metadata:WyIwYWhVS0V3alZ5WmZMaXZtSkF4WHd1bU1HSFZYREZPUVE4QmNJTmlnQSIsbnVsbCwyXQ=="/>
            //	<div class="rWbY0d"/>
            //	<div class="bfdHYd Ppzolf OFBs3e  ">
            //		<div class="rgFiGf OyjIsf "/>
            //		<div class="hHbUWd"/>
            //		<div class="rSy5If"/>
            //		<div class="lI9IFe ">
            //			<div class="y7PRA">
            //			</div>
            //			<div class="Rwjeuc"/>
            //			<div class="SpFAAb">
            //			</div>
            //			<div class="qty3Ue">
            //				<div class="AyRUI" aria-hidden="true" style="height: 8px;">&nbsp; </div>
            //				<div class="n8sPKe fontBodySmall ccePVe ">
            //					<div class="Ahnjwc fontBodyMedium ">
            //						<div class="W6VQef ">
            //							<div aria-hidden="true" class="JoXfOb fCbqBc" style="width: 16px; height: 16px;">
            //								<img alt="" class="Jn12ke xcEj5d " src="https://ssl.gstatic.com/local/servicebusiness/default_user.png" style="width: 16px; height: 16px;"/>
            //								<div class="ah5Ghc ">
            //									<span style="font-weight: 400;">"There is a separate </span>
            //									<span style="font-weight: 500;">gluten</span>
            //									<span style="font-weight: 400;"> free menu and the staff was very nice."</span>
            //								</div>
            //							</div>
            //							<div class="Q4BGF"/>
            //						</div>
            //					</div>
            //				</div>
            //				<div class="gwQ6lc" jsaction="click:mLt3mc"/>
            //			</div>
            //		</div>
            //	</div>
        }


        public void TraverseHtml(string html, LabelNode rootNode)
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
                    if (attribute.Name == "href")
                    {
                        currentParent.Href = attribute.Value;
                    }

                    if (attribute.Name == "aria-label")
                    {
                        var resultsNode = (attribute.Value.StartsWith("Results for"));

                        currentParent = new LabelNode
                        {
                            Name = attribute.Value,
                            ResultsNode = resultsNode
                        };
                        parent.Child.Add(currentParent);
                    }

                    if (attribute.Name == "aria-hidden")
                    {
                        currentParent.AriaHidden.Add(node.InnerHtml);
                    }
                }
            }

            if (node.Name == "span" && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                currentParent.Spans.Add(node.InnerText);
            }


            if (node.Name == "button" && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                currentParent.Buttons.Add(node.InnerText);
            }


            if (parent.ResultsNode)
            {
                var newParent = new LabelNode
                {
                    Name = "div",
                    InnerHtml = node.InnerHtml
                };
                currentParent.Child.Add(newParent);
                currentParent = newParent;
            }

            // Recurse through child nodes
            foreach (var childNode in node.ChildNodes)
            {
                TraverseNode(childNode, depth + 1, currentParent);
            }
        }

    }
}
