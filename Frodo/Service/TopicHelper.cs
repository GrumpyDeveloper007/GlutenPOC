using Frodo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal class TopicHelper
    {
        public Response GetOrCreateResponse(Topic currentTopic, string nodeId)
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

        public Topic GetOrCreateTopic(List<Topic> topics, string nodeId, string messageText)
        {
            Topic? currentTopic = null;
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
                currentTopic = new Topic()
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
