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
using Newtonsoft.Json;
using System.Net;

namespace TheShire
{
    public class MapHomeFunction(ILogger<MapHomeFunction> _logger, CloudDataStore _dataStore)
    {

        [Function("MapHome")]
        [OpenApiOperation(operationId: "MapHome")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        public async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
        {

            var content = await new StreamReader(request.Body).ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<MapHomeDb>(content);
            _logger.LogInformation("MapHome.Post");

            await _dataStore.ReplaceItemAsync<MapHomeDb>(data);

            return new OkObjectResult("");
        }
    }
}
