using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Homecloud.Contracts.Messages;
using Homecloud.Contracts.Requests;
using Homecloud.Contracts.Responses;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Homecloud
{
    [StorageAccount("DataStorage")]
    public static class FunctionsPipeline
    {
        [FunctionName("UpdatePipelinesRest")]
        public static PipelineUpdatingResponse SendUpdatePipelinesCommand(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UpdatePipelines")] UpdatePipelineRequest updatePipelineRequest,
            [Queue("update-pipelines")] out UpdatePipelinesCommand updatePipelinesCommand,
            ILogger logger)
        {
            logger.LogInformation("SendUpdatePipelinesCommand - C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(updatePipelineRequest)}");

            updatePipelinesCommand = new(updatePipelineRequest.Hash, updatePipelineRequest.ApiUrl);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(updatePipelinesCommand)}");

            return new PipelineUpdatingResponse { Hash = updatePipelineRequest.Hash };
        }

        [FunctionName("UpdatePipelines")]
        public static async Task ProcessUpdatePipelinesCommand(
            [QueueTrigger("update-pipelines")] UpdatePipelinesCommand updatePipelinesCommand,
            IBinder binder,
            ILogger logger)
        {
            logger.LogInformation($"ProcessUpdatePipelinesCommand - C# Queue trigger function processing command: {JsonConvert.SerializeObject(updatePipelinesCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, updatePipelinesCommand.ApiUrl, logger);
            var pipelines = await client.ListPipelines();
            logger.LogInformation("Pipelines data received");

            using var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"pipelines-data/{updatePipelinesCommand.ProjectHash}", FileAccess.Write));
            await writer.WriteAsync(JsonConvert.SerializeObject(pipelines));

            logger.LogInformation("Pipelines data saved");
        }
    }
}
