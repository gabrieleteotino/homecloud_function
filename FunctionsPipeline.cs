using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Homecloud.Contracts.Commands;
using Homecloud.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Homecloud
{
    [StorageAccount("DataStorage")]
    public static class FunctionsPipeline
    {
        [FunctionName("RefreshPipelinesRest")]
        public static IActionResult SendRefreshPipelinesCommand(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UpdatePipelines")] RefreshPipelineRequest updatePipelineRequest,
            [Queue("refresh-pipelines")] out RefreshPipelinesCommand refreshPipelinesCommand,
            ILogger logger)
        {
            logger.LogInformation("SendUpdatePipelinesCommand - C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(updatePipelineRequest)}");

            refreshPipelinesCommand = new(updatePipelineRequest.ProjectHash, updatePipelineRequest.ApiUrl);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(refreshPipelinesCommand)}");

            return new OkResult();
        }

        [FunctionName("RefreshPipelines")]
        public static async Task ProcessUpdatePipelinesCommand(
            [QueueTrigger("refresh-pipelines")] RefreshPipelinesCommand refreshPipelinesCommand,
            IBinder binder,
            ILogger logger)
        {
            logger.LogInformation($"ProcessUpdatePipelinesCommand - C# Queue trigger function processing command: {JsonConvert.SerializeObject(refreshPipelinesCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, refreshPipelinesCommand.ApiUrl, logger);
            var pipelines = await client.ListPipelines();
            logger.LogInformation("Pipelines data received");

            using var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"pipelines-data/{refreshPipelinesCommand.ProjectHash}", FileAccess.Write));
            await writer.WriteAsync(JsonConvert.SerializeObject(pipelines));

            logger.LogInformation("Pipelines data saved");

            // TODO send message 
        }
    }
}
