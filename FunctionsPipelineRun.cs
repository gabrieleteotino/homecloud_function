using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Data.Tables;
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
            [QueueTrigger("refresh-pipeline-runs")] RefreshPipelineRunsCommand refreshPipelineRunsCommand,
            IBinder binder,
            [Table("runs")] IAsyncCollector<TableEntity> runCollector,
            ILogger logger
        )
        {
            logger.LogInformation($"ProcessRefreshPipelineRunsCommand - C# Queue trigger function processing command: {JsonConvert.SerializeObject(refreshPipelineRunsCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, refreshPipelineRunsCommand.ApiUrl, logger);
            var pipelineRuns = await client.ListRuns(refreshPipelineRunsCommand.PipelineId);

            logger.LogInformation("Pipeline runs data received");

            using var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"pipeline-runs-data/{refreshPipelineRunsCommand.ProjectHash}/{refreshPipelineRunsCommand.PipelineId}.json", FileAccess.Write));
            await writer.WriteAsync(pipelineRuns);

            logger.LogInformation("Pipeline runs data saved");

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            using JsonDocument runs = JsonDocument.Parse(pipelineRuns);
            var value = runs.RootElement.GetProperty("value");
              foreach (var runElement in value.EnumerateArray())
            {
                var run = new Models.Devops.Run(
                    ProjectHash: refreshPipelineRunsCommand.ProjectHash,
                    PipelineId: runElement.GetProperty("pipeline").GetProperty("id").GetInt32(),
                    Id: runElement.GetProperty("id").GetInt32(),
                    Name: runElement.GetProperty("name").GetString(),
                    State: runElement.GetProperty("state").GetString(),
                    Result: runElement.GetProperty("result").GetString()
                );

                await runCollector.AddAsync(new TableEntity(run.ProjectHash, run.Id.ToString())
                {
                    ["Json"] = JsonConvert.SerializeObject(run)
                });
            }
            sw.Stop();
            logger.LogInformation($"Pipeline runs processed in {sw.ElapsedMilliseconds}ms");
        }
    }
}
