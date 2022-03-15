using System;
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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Project")] UpdatePipelineRequest updatePipelineRequest,
            [Queue("pdate-pipelines")] out UpdatePipelinesCommand updatePipelinesCommand,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(updatePipelineRequest)}");

            updatePipelinesCommand = new(updatePipelineRequest.Hash, updatePipelineRequest.ApiUrl);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(updatePipelinesCommand)}");

            return new PipelineUpdatingResponse { Hash = updatePipelineRequest.Hash };
        }

        [FunctionName("UpdatePipelines")]
        public static async Task Run(
            [QueueTrigger("update-pipelines")] UpdatePipelinesCommand updatePipelinesCommand,
            [Blob("devops-api-raw-data/{ProjectHash}.json", FileAccess.Write)] string blobResponse,
            ILogger logger)
        {
            logger.LogInformation($"C# Queue trigger function processing command: {JsonConvert.SerializeObject(updatePipelinesCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, updatePipelinesCommand.ApiUrl, logger);
            client.OnDataReceived(response => blobResponse = response);
            var pipelines = await client.ListPipelines();
            logger.LogInformation("Pipelines data received");
        }
    }
}
