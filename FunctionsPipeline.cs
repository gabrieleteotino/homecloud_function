using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Homecloud.Contracts.Commands;
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
            [Queue("update-pipelines")] out RefreshPipelinesCommand updatePipelinesCommand,
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
            [QueueTrigger("update-pipelines")] RefreshPipelinesCommand updatePipelinesCommand,
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

        [FunctionName("RefreshPipelineRunsRest")]
        public static async Task SendRefreshPipelineRunsCommand(
                    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "RefreshPipelineRuns")] RefreshPipelineRunsRequest refreshPipelineRunsRequest,
                    [Queue("refresh-pipeline-runs")] IAsyncCollector<RefreshPipelineRunsCommand> messageCollector,
                    ILogger logger)
        {
            logger.LogInformation("SendUpdatePipelinesCommand - C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(refreshPipelineRunsRequest)}");

            RefreshPipelineRunsCommand downloadPipelineRunsCommand = new(refreshPipelineRunsRequest.ProjectHash, refreshPipelineRunsRequest.ApiUrl, refreshPipelineRunsRequest.PipelineId);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(downloadPipelineRunsCommand)}");
            await messageCollector.AddAsync(downloadPipelineRunsCommand);
            return;
        }

        [FunctionName("RefreshPipelineRuns")]
        public static async Task ProcessRefreshPipelineRunsCommand(
            [QueueTrigger("refresh-pipeline-runs")] RefreshPipelineRunsCommand downloadPipelineRunsCommand,
            ILogger logger
        )
        {
            logger.LogInformation($"ProcessDownloadPipelineRunsCommand - C# Queue trigger function processing command: {JsonConvert.SerializeObject(downloadPipelineRunsCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, downloadPipelineRunsCommand.ApiUrl, logger);
            var pipelineRuns = await client.ListRuns(downloadPipelineRunsCommand.PipelineId);

            logger.LogInformation($"Downloaded pipeline {downloadPipelineRunsCommand.PipelineId} runs {pipelineRuns.Count()}");
        }
    }
}
