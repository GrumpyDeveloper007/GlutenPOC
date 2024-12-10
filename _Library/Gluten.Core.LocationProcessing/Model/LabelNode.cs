// Ignore Spelling: Href

namespace Gluten.Core.LocationProcessing.Model
{
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

}
