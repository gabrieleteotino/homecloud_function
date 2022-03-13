using System;
using System.Collections.Generic;
using System.IO;
using Homecloud.Contracts.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Homecloud
{
    public class UpdatePipelines
    {
        [FunctionName("UpdatePipelines")]
        internal void Run(
            [QueueTrigger("pipeline", Connection = "DataStorage")] PipelineListUpdate message,
            [Blob("pipeline-updates/{BlobId}", FileAccess.Read, Connection = "DataStorage")] List<Models.DevopsApi.Pipeline> pipelines,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {message}");
            log.LogInformation($"BlobId: {message.BlobId}");
        }
    }
}
