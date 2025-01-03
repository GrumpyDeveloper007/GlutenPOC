using Gluten.Core.LocationProcessing.Model;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Helper
{
    /// <summary>
    /// Html parsing helper 
    /// </summary>
    public static class HtmlHelper
    {
        /// <summary>
        /// Tries to get a restaurant type by rejecting what is not a restaurant type
        /// </summary>
        public static string GetRestaurantType(HtmlNodeCollection? spanNodes)
        {
            if (spanNodes == null) return "";
            foreach (var span in spanNodes)
            {
                if (string.IsNullOrWhiteSpace(span.InnerText)) continue;
                if (span.InnerText.Contains("review")) continue;
                if (span.InnerText.Contains('(')) continue;
                if (span.InnerText.Contains('$')) continue;
                if (span.InnerText.StartsWith(" · ")) continue;
                if (span.InnerText.Contains("–1")) continue;
                if (span.InnerText.Contains("–2")) continue;
                if (span.InnerText.Contains("–3")) continue;
                if (span.InnerText.Contains("–4")) continue;
                if (span.InnerText.Contains("–5")) continue;
                if (span.InnerText.Contains("–6")) continue;
                if (span.InnerText.Contains("–7")) continue;
                if (span.InnerText.Contains("–8")) continue;
                if (span.InnerText.Contains("–9")) continue;
                if (CurrencyHelper.ContainsCurrencySymbol(span.InnerText)) continue;
                if (span.InnerText.StartsWith("Price")) continue;

                return span.InnerText;
            }

            if (spanNodes.Count > 5)
            {
                return spanNodes[5].InnerText;
            }
            return "";
        }

        /// <summary>
        /// Get span nodes from the html string
        /// </summary>
        public static HtmlNodeCollection? GetSpanNodes(string html)
        {
            HtmlDocument document = new();
            document.LoadHtml(html);

            return document.DocumentNode.SelectNodes("//span[last()]");
        }

        /// <summary>
        /// Searches for a interesting comment (contains 'gluten')
        /// </summary>
        public static string GetComment(HtmlNodeCollection? spanNodes)
        {
            if (spanNodes == null) return "";
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

        /// <summary>
        /// Extracts interesting information from a html string and puts it in to a LabelNode structure
        /// </summary>
        public static void TraverseHtml(string html, LabelNode rootNode)
        {
            // Load the HTML into an HtmlDocument
            HtmlDocument document = new();
            document.LoadHtml(html);

            // Get the root node
            HtmlNode root = document.DocumentNode;

            // Traverse all nodes
            TraverseNode(root, 0, rootNode);
        }

        /// <summary>
        /// Loads the html string into an HtmlDocument
        /// </summary>
        public static HtmlDocument LoadHtml(string html)
        {
            // Load the HTML into an HtmlDocument
            HtmlDocument document = new();
            document.LoadHtml(html);
            return document;
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
