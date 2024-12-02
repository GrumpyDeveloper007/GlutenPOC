using Gluten.Data.Access.DatabaseModel;
using Gluten.Data.Access.Service;
using Gluten.Data.ClientModel;
using Gluten.Data.MapsModel;
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
    public class GMapsPinFunction(ILogger<GMapsPinFunction> _logger, CloudDataStore _dataStore)
    {
        [Function("GMapsPin")]
        [OpenApiOperation(operationId: "GMapsPin")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "multipart/form-data", bodyType: typeof(List<GMapsPin>), Description = "GMaps Pin array")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request, [FromQuery] string country)
        {
            List<GMapsPin> responseData = [];

            _logger.LogInformation("GMapsPin.Get");

            if (!string.IsNullOrWhiteSpace(country))
                responseData = await _dataStore.GetData<GMapsPinDb, GMapsPin>($"WHERE c.Country=\"{country}\"");

            return new OkObjectResult(responseData);
        }
    }
}
