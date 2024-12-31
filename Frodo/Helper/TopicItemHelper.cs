using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Helper
{
    /// <summary>
    /// Provides helper functions for TopicItem data class
    /// </summary>
    public class TopicItemHelper
    {
        /// <summary>
        /// Checks to see if the topic text is a question
        /// </summary>
        public static bool IsTopicAQuestion(DetailedTopic topic)
        {
            if (topic.TitleCategory == "QUESTION")
            {
                // AI says yes
                if (topic.Title.Contains('?')) return true;
                //if (topic.Title.Contains("Question", StringComparison.InvariantCultureIgnoreCase))                return true;
            }
            return false;
        }
        /// <summary>
        /// Checks to see if the topic text is a recipe
        /// </summary>
        public static bool IsRecipe(DetailedTopic topic)
        {
            if (topic.Title.Contains("Recipe", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (topic.Title.Contains("INGREDIENTS", StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }
    }
}
