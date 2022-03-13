using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Homecloud
{
    public class DevopsTimer
    {
        [FunctionName("DevopsTimer")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
