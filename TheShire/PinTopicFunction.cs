using Gluten.Data.Access.DatabaseModel;
using Gluten.Data.Access.Service;
using Gluten.Data.ClientModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace TheShire
{
    public class PinTopicFunction(ILogger<PinTopicFunction> _logger, DataStore _dataStore)
    {
        [Function("PinTopic")]
        [OpenApiOperation(operationId: "PinTopic")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "multipart/form-data", bodyType: typeof(List<PinTopic>), Description = "Pin Topic array")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request, [FromQuery] string region)
        {
            _logger.LogInformation("PinTopic.Get");

            var responseData = _dataStore.GetData();

            return new OkObjectResult(responseData);
        }

        [Function("PinTopic")]
        [OpenApiOperation(operationId: "PinTopic")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public IActionResult Post([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request, PinTopicDb topic)
        {
            _logger.LogInformation("PinTopic.Post");

            var responseData = _dataStore.ReplacePinTopicDbItemAsync(topic);

            return new OkObjectResult(responseData);
        }
    }
}
