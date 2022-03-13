using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Homecloud.Contracts.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Homecloud
{
    [StorageAccount("DataStorage")]
    public static class FunctionsPipeline
    {
        [FunctionName("UpdatePipelines")]
        public static async Task Run(
            [QueueTrigger("update-pipelines")] UpdatePipelines updatePipelinesCommand,
            ILogger logger)
        {

            logger.LogInformation($"C# Queue trigger function processing command: {JsonConvert.SerializeObject(updatePipelinesCommand)}");
            var personalAccessToken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            var client = new DevopsApiClient(personalAccessToken, updatePipelinesCommand.ApiUrl, logger);
            var pipelines = await client.ListPipelines();
            
        }
    }
}
