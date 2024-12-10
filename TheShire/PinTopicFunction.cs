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
    public class PinTopicFunction(ILogger<PinTopicFunction> _logger, CloudDataStore _dataStore)
    {
        [Function("PinTopic")]
        [OpenApiOperation(operationId: "PinTopic")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "multipart/form-data", bodyType: typeof(List<PinTopic>), Description = "Pin Topic array")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequest request,
            [FromQuery] string country)
        {
            List<PinTopic> responseData = [];
            _logger.LogInformation("PinTopic.Get");

            if (!string.IsNullOrWhiteSpace(country))
                responseData = await _dataStore.GetData<PinTopicDb, PinTopic>($"WHERE c.Country=\"{country}\"");

            return new OkObjectResult(responseData);
        }

        //[Function("PinTopic")]
        //[OpenApiOperation(operationId: "PinTopic")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //public IActionResult Post([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request, PinTopicDb topic)
        //{
        //    _logger.LogInformation("PinTopic.Post");

        //    var responseData = _dataStore.ReplacePinTopicDbItemAsync(topic);

        //    return new OkObjectResult(responseData);
        //}
    }
}
