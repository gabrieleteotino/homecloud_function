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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "RefreshPipelines")] RefreshPipelineRequest refreshPipelineRequest,
            [Queue("refresh-pipelines")] out RefreshPipelinesCommand refreshPipelinesCommand,
            ILogger logger)
        {
            logger.LogInformation("SendRefreshPipelinesCommand - C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(refreshPipelineRequest)}");

            refreshPipelinesCommand = new(refreshPipelineRequest.ProjectHash, refreshPipelineRequest.ApiUrl);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(refreshPipelinesCommand)}");

            return new OkResult();
        }

        [FunctionName("RefreshPipelines")]
        public static async Task ProcessRefreshPipelinesCommand(
            [QueueTrigger("refresh-pipelines")] RefreshPipelinesCommand refreshPipelinesCommand,
            IBinder binder,
            ILogger logger)
        {
            logger.LogInformation($"ProcessRefreshPipelinesCommand - C# Queue trigger function processing command: {JsonConvert.SerializeObject(refreshPipelinesCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, refreshPipelinesCommand.ApiUrl, logger);
            var pipelines = await client.ListPipelines();
            logger.LogInformation("Pipelines data received");

            using var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"pipelines-data/{refreshPipelinesCommand.ProjectHash}.json", FileAccess.Write));
            await writer.WriteAsync(JsonConvert.SerializeObject(pipelines));

            logger.LogInformation("Pipelines data saved");

            // TODO send message 
        }
    }
}
