using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace homecloud_function
{
    public class DevopsTimer
    {
        [FunctionName("DevopsTimer")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        private async Task<List<string>> ListPipelines(string organization, string project)
        {
            var personalaccesstoken = "PAT";

            var uri = $"GET https://dev.azure.com/{organization}/{project}/_apis/pipelines?api-version=6.0-preview.1";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalaccesstoken))));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            //Read Server Response
            HttpResponseMessage response = await client.SendAsync(request);
            bool isValidMpn = await response.Content.ReadAsAsync<bool>();
        }
    }
}
