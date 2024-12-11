using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.Helper
{
    /// <summary>
    /// Some helper function for working with topic classes
    /// </summary>
    public static class TopicHelper
    {
        /// <summary>
        /// Checks to see if a response exists for the given nodeId, selects it or creates a new one
        /// </summary>
        public static Response GetOrCreateResponse(DetailedTopic currentTopic, string nodeId)
        {
            Response? currentResponse = null;
            foreach (var response in currentTopic.ResponsesV2)
            {
                if (response.NodeId == nodeId)
                {
                    currentResponse = response;
                    break;
                }
            }
            if (currentResponse == null)
            {
                currentResponse = new Response()
                {
                    NodeId = nodeId,
                };
                currentTopic.ResponsesV2.Add(currentResponse);
            }
            return currentResponse;

        }

        /// <summary>
        /// Checks to see if a topic exists for the given nodeId, selects it or creates a new one
        /// </summary>
        public static DetailedTopic GetOrCreateTopic(List<DetailedTopic> topics, string nodeId, string messageText)
        {
            DetailedTopic? currentTopic = null;
            foreach (var topic in topics)
            {
                if (topic.NodeID == nodeId)
                {
                    currentTopic = topic;
                    break;
                }
            }
            if (currentTopic == null)
            {
                currentTopic = new DetailedTopic()
                {
                    NodeID = nodeId,
                    Title = messageText,
                };
                topics.Add(currentTopic);
            }
            return currentTopic;
        }


    }
}
