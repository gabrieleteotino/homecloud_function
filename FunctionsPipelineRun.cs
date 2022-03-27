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
    public static class FunctionsPipelineRun
    {
        [FunctionName("RefreshPipelineRunsRest")]
        public static async Task<IActionResult> SendRefreshPipelineRunsCommand(
                    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "RefreshPipelineRuns")] RefreshPipelineRunsRequest refreshPipelineRunsRequest,
                    [Queue("refresh-pipeline-runs")] IAsyncCollector<RefreshPipelineRunsCommand> messageCollector,
                    ILogger logger)
        {
            logger.LogInformation("SendUpdatePipelinesCommand - C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(refreshPipelineRunsRequest)}");

            RefreshPipelineRunsCommand downloadPipelineRunsCommand = new(refreshPipelineRunsRequest.ProjectHash, refreshPipelineRunsRequest.ApiUrl, refreshPipelineRunsRequest.PipelineId);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(downloadPipelineRunsCommand)}");
            await messageCollector.AddAsync(downloadPipelineRunsCommand);
            return new OkResult();
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
